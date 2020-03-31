<template>
  <div id="app-sel"> 
    <multi-select ref="appSelection" v-model="selectedApp" :limit="1" 
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
				selectedApp: []
			}
		},
		computed: {
			appIsSelected() {
				return this.selectedApp.length > 0;
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
            changeApp() {
                var app =_.find(this.applications, (a) => a.id == this.selectedApp[0]);
				if (app) {
                    Bus.$emit(events.applicationChanged, app);
                    localStorage.setItem('selectedApp', app.id);
					this.$refs.appSelection.showDropdown = false;
				}
			},
            getApplications() {
				axios.get('/api/applications')
					.then((response) => {
						this.applications = response.data
						if (!this.appIsSelected) {
							if (response.data.length > 0) {
                                if (this.selectedApp.length == 0) {
                                    if (localStorage.getItem('selectedApp') === null || !this.existsStoredApp()) {
                                        this.selectedApp.push(response.data[0].id);
                                    }
                                    else {
                                        this.selectedApp.push(localStorage.getItem('selectedApp'));
                                    }
                                    this.changeApp();
								}
							}
						}
					})
					.catch(error => { window.alert(error) })
            },
			refreshApps() {
				this.selectedApp = [];
				this.getApplications();
            },
            getAllAplications() {
                axios.get('/api/applications')
                    .then((response) => {
                        this.application = response.data;
                    })
                    .catch(error => { window.alert(error) });
            },
            existsStoredApp() {
                var app = _.find(this.applications, (a) => a.id == localStorage.getItem('selectedApp'));
                return app != null 
            }
		}
    }
</script>