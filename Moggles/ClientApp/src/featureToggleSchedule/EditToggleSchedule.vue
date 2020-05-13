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
              <input id="s1" v-model="schedule.scheduledState" type="radio"
                     :value="true"> On
            </label>
            <label for="s2">
              <input id="s2" v-model="schedule.scheduledState" type="radio"
                     :value="false"> Off
            </label>
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label" for="toggleSelect">Toggle name</label>
          <div class="col-sm-8">
            <input v-model="schedule.toggleName" type="text" class="form-control"
                   disabled>
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label" for="environmentsSelect">Select Environments</label>
          <div class="col-sm-8">
            <multi-select id="environmentsSelect" v-model="schedule.environments" name="environmentsSelect"
                          :options="allEnvironments" block :selected-icon="'fas fa-deactivate'" />
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label">Select Change State Date/Time</label>
          <div class="col-sm-8">
            <dropdown class="col-sm-7 margin-top-9 form-group">
              <div class="input-group">
                <input id="dateInput" v-model="schedule.scheduledDate" class="form-control"
                       type="text" readonly="readonly">
                <div class="input-group-btn">
                  <btn class="dropdown-toggle">
                    <i class="fas fa-calendar" />
                  </btn>
                </div>
              </div>
              <template slot="dropdown">
                <li>
                  <date-picker v-model="schedule.scheduledDate" :icon-control-left="'fas fa-angle-left'" :icon-control-right="'fas fa-angle-right'" />
                </li>
              </template>
            </dropdown>
            <dropdown class="col-sm-7 margin-top-9 form-group">
              <div class="input-group">
                <input id="timeInput" class="form-control" type="text"
                       :value="schedule.scheduledTime.toTimeString()" readonly="readonly">
                <div class="input-group-btn">
                  <btn class="dropdown-toggle">
                    <i class="fas fa-clock" />
                  </btn>
                </div>
              </div>
              <template slot="dropdown">
                <li style="padding: 5px">
                  <time-picker v-model="schedule.scheduledTime" :icon-control-up="'fas fa-angle-up'" :icon-control-down="'fas fa-angle-down'" />
                </li>
              </template>
            </dropdown>
          </div>
        </div>
        <div v-if="isCacheRefreshEnabled" class="col-sm-12 form-group">
          <label class="col-sm-4 control-label" for="cacheRefresh">Force Cache Refresh</label>
          <span class="col-sm-2 margin-top-6">
            <p-check v-model="schedule.forceCacheRefresh" class="p-icon p-fill" name="cacheRefresh"
                     color="default">
              <i slot="extra" class="icon fas fa-check" />
            </p-check>
          </span>
        </div>
        <div class="col-sm-12 clearfix">
          <div class="pull-left">
            <button type="button" class="btn btn-danger" @click="showDeleteConfirmationModal">
              Delete
            </button>
          </div>
          <div class="pull-right">
            <button id="closeButton" class="btn btn-default" @click="closeModal">
              Close
            </button>
            <button id="submitButton" class="btn btn-primary" type="button"
                    @click="editSchedule">
              Submit
            </button>
          </div>
        </div>
      </div>
    </div>
    <modal v-if="showDeleteConfirmation" v-model="showDeleteConfirmation" title="You are about to delete a feature toggle schedule"
           :footer="false"
           append-to-body>
      <deleteToggleSchedule :schedule="schedule" />
    </modal>
  </div>
</template>

<script>
    import { Bus } from '../common/event-bus'
    import { events } from '../common/events';
    import axios from 'axios'
    import moment from 'moment';
    import _ from 'lodash';
    import PrettyCheck from 'pretty-checkbox-vue/check';
    import DeleteToggleSchedule from './DeleteToggleSchedule';


    export default {
        components: {
            'p-check': PrettyCheck,
            'deleteToggleSchedule': DeleteToggleSchedule
        },
        props: {
            isCacheRefreshEnabled: {
                type: Boolean,
                required: true
            },
            application: {
                type: Object,
                required: true
            },
            schedule: {
                type: Object,
                required: true
            }
        },
        data() {
            return {
                errors: [],
                allEnvironments: [],
                showDeleteConfirmation: false
            }
        },
        computed: {
            scheduledDateTime() {
                let scheduledDateFormat = moment(this.schedule.scheduledDate).format("YYYY-MM-DD");
                let scheduledTimeFormat = moment(this.schedule.scheduledTime).format("hh:mm:ss A");
                return new Date(`${scheduledDateFormat} ${scheduledTimeFormat}`);
            }
        },
        created() {
            this.loadEnvironments();

            Bus.$on(events.closeDeleteSchedulerModal, () => {
                this.showDeleteConfirmation = false;
            });
        },
        methods: {
            showDeleteConfirmationModal() {
                this.showDeleteConfirmation = true;
            },
            editSchedule() {
                Bus.$emit(events.blockUI)

                this.validateSchedule();
                if (this.errors.length > 0) {
                    Bus.$emit(events.unblockUI)
                    return;
                }

                axios.put('api/ToggleScheduler', {
                    toggleName: this.schedule.toggleName,
                    id: this.schedule.scheduleId,
                    scheduledState: this.schedule.scheduledState,
                    scheduledDate: this.scheduledDateTime,
                    environments: this.schedule.environments,
                    forceCacheRefresh: this.schedule.forceCacheRefresh
                }).then(() => {
                    this.$notify({
                        type: "success",
                        content: "Success updating a scheduled feature toggle!",
                        offsetY: 70,
                        icon: 'fas fa-check-circle'
                    })
                    this.closeModal();
                    Bus.$emit(events.toggleScheduled);
                }).catch(e => {
                    this.$notify({
                        type: "error",
                        content: "Error updating scheduled feature toggle: " + e,
                        offsetY: 70,
                        icon: 'fas fa-check-circle'
                    });
                }).finally(() => {
                    Bus.$emit(events.unblockUI)
                });
            },
            validateSchedule() {
                this.errors = [];

                if (this.schedule.environments.length == 0) {
                    this.errors.push('You must select at least one environment!');
                }

                if (this.schedule.scheduledDate === null) {
                    this.errors.push('You must select a change state date and time!');
                }

                let currentDate = new Date(moment().format("YYYY-MM-DD hh:mm:ss A"));
                if (this.scheduledDateTime < currentDate) {
                    this.errors.push("Please select a change state date and time in the future!");
                }
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
                }).catch((e) => { window.alert(e) });
            },
            closeModal() {
                Bus.$emit(events.closeToggleSchedulerModal);
            }
        }
    }
</script>