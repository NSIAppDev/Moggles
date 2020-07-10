<template>
  <div>
    <div class="form-horizontal">
      <div class="row">
        <div v-for="error in errors" :key="error" class="text-danger margin-bottom-10">
          {{ error }}
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label">Select State</label>
          <div class="col-sm-6 margin-top-6 form-inline">
            <label for="s1">
              <input id="s1" v-model="scheduledState" type="radio"
                     :value="true"> On
            </label>
            <label for="s2">
              <input id="s2" v-model="scheduledState" type="radio"
                     :value="false"> Off
            </label>
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label" for="toggleSelect">Select Toggles</label>
          <div class="col-sm-8">
            <multi-select id="toggleSelect" v-model="selectedToggles"
                          type="text" name="toggleSelect"
                          :options="allToggles" block :selected-icon="'fas fa-check'" />
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label" for="environmentsSelect">Select Environments</label>
          <div class="col-sm-8">
            <multi-select id="environmentsSelect" v-model="selectedEnvironments" name="environmentsSelect"
                          :options="allEnvironments" block :selected-icon="'fas fa-deactivate'" />
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label">Select Change State Date/Time</label>
          <div class="col-sm-8">
            <dropdown class="col-sm-7 margin-top-9 form-group">
              <div class="input-group">
                <input id="dateInput" v-model="scheduledDate" class="form-control"
                       type="text" readonly="readonly">
                <div class="input-group-btn">
                  <btn class="dropdown-toggle">
                    <i class="fas fa-calendar" />
                  </btn>
                </div>
              </div>
              <template slot="dropdown">
                <li>
                  <date-picker v-model="scheduledDate" :icon-control-left="'fas fa-angle-left'" :icon-control-right="'fas fa-angle-right'" />
                </li>
              </template>
            </dropdown>
            <dropdown class="col-sm-7 margin-top-9 form-group">
              <div class="input-group">
                <input id="timeInput" class="form-control" type="text"
                       :value="scheduledTime.toTimeString()" readonly="readonly">
                <div class="input-group-btn">
                  <btn class="dropdown-toggle">
                    <i class="fas fa-clock" />
                  </btn>
                </div>
              </div>
              <template slot="dropdown">
                <li style="padding: 5px">
                  <time-picker v-model="scheduledTime" :icon-control-up="'fas fa-angle-up'" :icon-control-down="'fas fa-angle-down'" />
                </li>
              </template>
            </dropdown>
          </div>
        </div>
        <div v-if="isCacheRefreshEnabled" class="col-sm-12 form-group">
          <label class="col-sm-4 control-label" for="cacheRefresh">Force Cache Refresh</label>
          <span class="col-sm-2 margin-top-6">
            <p-check v-model="forceCacheRefresh" class="p-icon p-fill" name="cacheRefresh"
                     color="default">
              <i slot="extra" class="icon fas fa-check" />
            </p-check>
          </span>
        </div>
        <div class="col-sm-12 clearfix">
          <div class="pull-right">
            <button id="closeButton" class="btn btn-default" @click="closeModal">
              Close
            </button>
            <button id="submitButton" class="btn btn-primary" type="button"
                    @click="addSchedule">
              Submit
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
    import { Bus } from '../common/event-bus'
    import axios from 'axios'
    import moment from 'moment';
    import _ from 'lodash';
    import PrettyCheck from 'pretty-checkbox-vue/check';
    import { events } from '../common/events';

    export default {
        components: {
            'p-check': PrettyCheck
        },
        props: {
            isCacheRefreshEnabled: {
                type: Boolean,
                required: true
            },
            application: {
                type: Object,
                required: true
            }
        },
        data() {
            return {
                scheduledState: true,
                selectedToggles: [],
                selectedEnvironments: [],
                scheduledDate: null,
                scheduledTime: new Date(),
                forceCacheRefresh: false,
                errors: [],
                allToggles: [],
                allEnvironments: []
            }
        },
        computed: {
            scheduledDateTime() {
                let scheduledDateFormat = moment(this.scheduledDate).format("YYYY-MM-DD");
                let scheduledTimeFormat = moment(this.scheduledTime).format("hh:mm:ss A");
                return new Date(`${scheduledDateFormat} ${scheduledTimeFormat}`);
            }
        },
        created() {
            this.loadToggles();
            this.loadEnvironments();
        },
        methods: {
            loadToggles() {
                axios.get("/api/FeatureToggles", {
                    params: {
                        applicationId: this.application.id
                    }
                }).then((response) => {
                    this.allToggles = _.map(response.data.filter(ft => ft.userAccepted == false), toggle => {
                        return {
                            value: toggle.toggleName,
                            label: toggle.toggleName
                        };
                    });
                }).catch(error => Bus.$emit(events.showErrorAlertModal, { 'error': error }));
            },
            loadEnvironments() {
                axios.get("/api/FeatureToggles/environments", {
                    params: {
                        applicationId: this.application.id
                    }
                }).then((response) => {
                    this.allEnvironments = _.map(response.data, env => {
                        return {
                            value: env.envName,
                            label: env.envName
                        };
                    });
                }).catch(error => Bus.$emit(events.showErrorAlertModal, { 'error': error }));
            },
            addSchedule() {
                Bus.$emit(events.blockUI);

                this.validateSchedule();
                if (this.errors.length > 0) {
                    Bus.$emit(events.unblockUI)
                    return;
                }

                axios.post('api/ToggleScheduler', {
                    applicationId: this.application.id,
                    state: this.scheduledState,
                    featureToggles: this.selectedToggles,
                    environments: this.selectedEnvironments,
                    scheduleDate: this.scheduledDateTime,
                    forceCacheRefresh: this.forceCacheRefresh
                }).then(() => {
                    this.$notify({
                        type: "success",
                        content: "Success scheduling feature toggle!",
                        offsetY: 70,
                        icon: 'fas fa-check-circle'

                    })
                    this.closeModal();
                    Bus.$emit(events.toggleScheduled);
                }).catch(e => {
                    this.$notify({
                        type: "error",
                        content: "Error scheduling feature: " + e,
                        offsetY: 70,
                        icon: 'fas fa-check-circle'
                    });
                }).finally(() => {
                    Bus.$emit(events.unblockUI)
                });
            },
            validateSchedule() {
                this.errors = [];

                if (this.selectedToggles.length == 0) {
                    this.errors.push('You must select at least one feature toggle!');
                }

                if (this.selectedEnvironments.length == 0) {
                    this.errors.push('You must select at least one environment!');
                }

                if (this.scheduledDate === null) {
                    this.errors.push('You must select a change state date and time!');
                }

                let currentDate = new Date(moment().format("YYYY-MM-DD hh:mm:ss A"));
                if (this.scheduledDateTime < currentDate) {
                    this.errors.push("Please select a change state date and time in the future!");
                }
            },
            closeModal() {
                Bus.$emit(events.closeToggleSchedulerModal);
            }
        }
    }
</script>