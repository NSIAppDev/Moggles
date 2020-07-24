<template>
  <div id="app-sel">
    <multi-select id="selectedApp" ref="appSelection" v-model="selectedAppId"
                  :limit="1"
                  :options="applications" :value-key="'id'" :label-key="'appName'"
                  :selected-icon="'fas fa-check'" class="padding-left-10" @change="changeApp" />
  </div>
</template>

<script>
    import { Bus } from '../common/event-bus';
    import { events } from '../common/events';
    import axios from 'axios';
    import _ from 'lodash';

    export default {
        data() {
            return {
                applications: [],
                selectedAppId: [],
                selectedAppIdLocalStorageKey: 'selectedAppId'
            }
        },
        created() {
            this.getApplications();

            Bus.$on(events.newApplicationAdded, () => {
                this.getApplications();
            });

            Bus.$on(events.applicationEdited, () => {
                this.getApplications();
            });

            Bus.$on(events.reloadApplicationToggles, () => {
                this.changeApp();
            });

            Bus.$on(events.refreshApplications, () => {
                this.refreshApps();
            });
        },
        methods: {
            getApplications() {
                axios.get('/api/applications')
                    .then((response) => {
                        this.applications = response.data;

                        if (!this.isApplicationSelected() && this.applications.length > 0) {
                            var localStorageSelectedApp = this.getSelectedAppFromLocalStorage();
                            if (localStorageSelectedApp != null) {
                                this.selectedAppId = [localStorageSelectedApp.id];
                            }
                            else {
                                this.selectedAppId = [this.applications[0].id];
                            }

                            this.setNewApplicationSelection();
                        }

                    }).catch(error => {
                        Bus.$emit(events.showErrorAlertModal, { 'error': error });
                    });
            },
            changeApp() {
                if (!this.isApplicationSelected()) {
                    this.getApplications();
                } else {
                    this.setNewApplicationSelection();
                }
            },
            setNewApplicationSelection() {
                let selectedApp = _.find(this.applications, (application) => application.id == this.selectedAppId[0]);
                Bus.$emit('block-ui')
                Bus.$emit(events.applicationChanged, selectedApp);
                localStorage.setItem(this.selectedAppIdLocalStorageKey, this.selectedAppId[0]);
                this.$refs.appSelection.showDropdown = false;
            },
            refreshApps() {
                this.selectedAppId = [];
                this.getApplications();
            },
            getApplication(applicationId) {
                return _.find(this.applications, (application) => application.id == applicationId);
            },
            getSelectedAppFromLocalStorage() {
                var localStorageSelectedAppId = localStorage.getItem(this.selectedAppIdLocalStorageKey);
                return this.getApplication(localStorageSelectedAppId);
            },
            isApplicationSelected() {
                return this.selectedAppId != null && this.selectedAppId.length > 0;
            }
        }
    }
</script>