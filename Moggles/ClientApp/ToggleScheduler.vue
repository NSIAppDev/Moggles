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
                        <multi-select v-if="existsTogggleSchedule(toggle)" id="toggleSelect" v-model="selectedToggles"
                                      name="toggleSelect"
                                      :options="allToggles" block :selected-icon="'fas fa-check'" />
                        <multi-select v-else id="toggleSelect" v-model="selectedToggles"
                                      type="text" name="toggleSelect"
                                      :options="allToggles" block :selected-icon="'fas fa-check'"
                                      disabled />
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
                        <p-check v-model="forceCacheRefresh" class="p-icon p-fill" name="cacheRefresh" color="default">
                            <i slot="extra" class="icon fas fa-check" />
                        </p-check>
                    </span>
                </div>
                <div class="col-sm-12 clearfix">
                    <div v-if="!existsTogggleSchedule(toggle)" class="pull-left">
                        <button type="button" class="btn btn-danger" @click="showConfirmDeleteModal">
                            Delete
                        </button>
                    </div>
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
        <modal v-model="showDeleteConfirmation" title="You are about to delete a feature toggle schedule" :footer="false"
               append-to-body>
            <deleteToggleScheduler/>
        </modal>
    </div>
</template>

<script>
    import { Bus } from './event-bus'
    import axios from 'axios'
    import moment from 'moment';
    import _ from 'lodash';
    import PrettyCheck from 'pretty-checkbox-vue/check';
    import DeleteToggleScheduler from './DeleteToggleScheduler'

    export default {
        components: {
            'p-check': PrettyCheck,
            'deleteToggleScheduler': DeleteToggleScheduler
        },
        data() {
            return {
                environmentName: "",
                scheduledState: true,
                errors: [],
                selectedAppId: "",
                allToggles: [],
                selectedToggles: [],
                allEnvironments: [],
                selectedEnvironments: [],
                scheduledDate: null,
                scheduledTime: new Date(),
                toggle: null,
                showDeleteConfirmation: false,
                forceCacheRefresh: false,
                isCacheRefreshEnabled: false
            }
        },
        created() {
            Bus.$on("app-changed", app => {
                if (app) {
                    this.toggle = null;
                    this.selectedAppId = app.id;
                    this.loadToggles(this.selectedAppId);
                    this.loadEnvironments(this.selectedAppId);
                }
            })
            Bus.$on("env-added", () => {
                this.loadEnvironments(this.selectedAppId);
            })

            Bus.$on("toggle-added", () => {
                this.loadToggles(this.selectedAppId);
            })

            Bus.$on("edit-schedule", toggleId => {
                this.selectedEnvironments = [];
                this.loadToggles(this.selectedAppId);
                this.loadToggleForEdit(toggleId);
                this.loadEnvironments(this.selectedAppId);
            })

            Bus.$on('add-scheduler', () => {
                this.cleanup();
                this.loadToggles(this.selectedAppId);
                this.loadEnvironments(this.selectedAppId);
                this.toggle = null;
            })

            Bus.$on('close-deleteScheduler', () => {
                this.showDeleteConfirmation = false;
                this.loadToggles(this.selectedAppId);
                this.loadEnvironments(this.selectedAppId);
            })

            axios.get("/api/CacheRefresh/getCacheRefreshAvailability").then((response) => {
                this.isCacheRefreshEnabled = response.data;
            }).catch(error => { window.alert(error) });
        },
        methods: {
            showConfirmDeleteModal() {
                this.showDeleteConfirmation = true;
                Bus.$emit('delete-scheduler', this.toggle);
            },
            existsTogggleSchedule(toggle) {
                return toggle === null ? true : false;
            },
            addSchedule() {
                this.errors = [];
                Bus.$emit('block-ui')

                if (this.selectedToggles.length == 0) {
                    this.errors.push('You must select at least one feature toggle');
                }

                if (this.selectedEnvironments.length == 0) {
                    this.errors.push('You must select at least one environment');
                }
                if (this.scheduledDate === null) {
                    this.errors.push('You must select a change state date and time');
                }

                let currentDate = new Date(moment().format("YYYY-MM-DD hh:mm:ss A"));
                let scheduledDateFormat = moment(this.scheduledDate).format("YYYY-MM-DD");
                let scheduledTimeFormat = moment(this.scheduledTime).format("hh:mm:ss A");
                let dateTime = scheduledDateFormat + " " + scheduledTimeFormat;
                let scheduledDateTime = new Date(dateTime);
                if (scheduledDateTime < currentDate) {
                    this.errors.push("Please select a change state date and time in the future!");
                }
                if (this.errors.length > 0) {
                    Bus.$emit('unblock-ui')
                    return;
                }

                let combinedScheduledDateTime = moment(this.scheduledDate);
                let time = moment(this.scheduledTime);
                combinedScheduledDateTime.add(time.hours(), 'hours');
                combinedScheduledDateTime.add(time.minutes(), 'minutes');
                if (this.existsTogggleSchedule(this.toggle)) {
                    axios.post('api/ToggleScheduler', {
                        applicationId: this.selectedAppId,
                        state: this.scheduledState,
                        featureToggles: this.selectedToggles,
                        environments: this.selectedEnvironments,
                        scheduleDate: combinedScheduledDateTime,
                        forceCacheRefresh: this.forceCacheRefresh
                    }).then(() => {
                        this.$notify({
                            type: "success",
                            content: "Success scheduling feature toggle!",
                            offsetY: 70,
                            icon: 'fas fa-check-circle'

                        })
                        this.cleanup();
                        this.closeModal();
                        Bus.$emit('toggle-scheduled');
                    }).catch(e => {
                        Bus.$emit('unblock-ui')
                        this.$notify({
                            type: "error",
                            content: "Error scheduling feature: " + e,
                            offsetY: 70,
                            icon: 'fas fa-check-circle'
                        });
                    }).finally(() => {
                        Bus.$emit('unblock-ui')
                    });
                }
                else {
                    axios.put('api/ToggleScheduler', {
                        toggleName: this.toggle.toggleName,
                        id: this.toggle.id,
                        scheduledState: this.scheduledState,
                        scheduledDate: combinedScheduledDateTime,
                        environments: this.selectedEnvironments,
                        forceCacheRefresh: this.forceCacheRefresh
                    }).then(() => {
                        this.$notify({
                            type: "success",
                            content: "Success updating a scheduled feature toggle!",
                            offsetY: 70,
                            icon: 'fas fa-check-circle'
                        })
                        this.cleanup();
                        this.closeModal();
                        Bus.$emit('toggle-scheduled');
                    }).catch(e => {
                        Bus.$emit('unblock-ui')
                        this.$notify({
                            type: "error",
                            content: "Error updating scheduled feature toggle: " + e,
                            offsetY: 70,
                            icon: 'fas fa-check-circle'
                        });
                    }).finally(() => {
                        Bus.$emit('unblock-ui')
                    });

                }
            },
            loadToggles(appId) {
                axios.get("/api/FeatureToggles", {
                    params: {
                        applicationId: appId
                    }
                }).then((response) => {
                    let dropDownModels = _.map(response.data.filter(ft => ft.userAccepted == false), toggle => {
                        return {
                            value: toggle.toggleName,
                            label: toggle.toggleName
                        };
                    });

                    this.allToggles = dropDownModels;
                }).catch(error => {
                    window.alert(error)
                });
            },
            loadEnvironments(appId) {
                axios.get("/api/FeatureToggles/environments", {
                    params: {
                        applicationId: appId
                    }
                }).then((response) => {
                    let dropDownModels = _.map(response.data, env => {
                        return {
                            value: env.envName,
                            label: env.envName
                        };
                    });
                    this.allEnvironments = dropDownModels;
                }).catch((e) => { window.alert(e) });
            },
            loadToggleForEdit(toggleId) {
                this.cleanup();
                axios.get('/api/ToggleScheduler/getToggleScheduler', {
                    params: {
                        toggleId: toggleId
                    }
                }).then((response) => {
                    this.toggle = response.data;
                    this.toggleName = this.toggle.toggleName;
                    this.selectedToggles.push(this.toggle.toggleName);
                    this.toggle.environments.forEach(env => this.selectedEnvironments.push(env));
                    this.scheduledDate = moment(this.toggle.scheduledDate).format("YYYY-MM-DD");
                    this.scheduledTime = new Date(this.toggle.scheduledDate);
                    this.scheduledState = this.toggle.scheduledState;
                    this.forceCacheRefresh = this.toggle.forceCacheRefresh;
                }).catch((e) => { window.alert(e) });

            },
            cleanup() {
                this.selectedToggles = [];
                this.selectedEnvironments = [];
                this.errors = [];
                this.scheduledState = true;
                this.scheduledDate = null;
                this.scheduledTime = new Date();
                this.toggle = null;
                this.toggleName = null;
                this.forceCacheRefresh = false;
            },
            closeModal() {
                this.cleanup();
                Bus.$emit('close-scheduler');
            }
        }
    }
</script>