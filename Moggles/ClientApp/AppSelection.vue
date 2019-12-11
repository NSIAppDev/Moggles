<template>
  <div id="app-sel"> 
    <multi-select ref="appSelection" v-model="selectedApps" :limit="1" 
                  :options="applicationList" :value-key="'id'" :label-key="'appName'"
                  :selected-icon="'fas fa-check'" class="padding-left-10" @change="changeApp" />
  </div>
</template>

<script>
	import { Bus } from './event-bus'
	import axios from 'axios'
	import _ from 'lodash'

	export default {
		data() {
			return {
				applicationList: [],
				selectedApps: []
			}
		},
		computed: {
			appIsSelected() {
				return this.selectedApps.length > 0;
			}
		},
		created() {
			this.getApplications()
			Bus.$on("new-app-added", () => {
				this.getApplications();
			});
			Bus.$on("reload-application-toggles", () => {
				this.changeApp();
			});
			Bus.$on("refresh-apps", () => {
				this.refreshApps();
			});
		},
		methods: {
			changeApp() {
				var app = _.find(this.applicationList, (a) => a.id == this.selectedApps[0])
				if (app) {
                    Bus.$emit('app-changed', app)
                    localStorage.setItem('selectedApp', app.id);
					this.$refs.appSelection.showDropdown = false
				}
			},
			getApplications() {
				axios.get('/api/applications')
					.then((response) => {
						this.applicationList = response.data
						if (!this.appIsSelected) {
							if (response.data.length > 0) {
								if (this.selectedApps.length == 0) {
									this.selectedApps.push(response.data[0].id);
									this.changeApp();
								}
							}
						}
					})
					.catch(error => { window.alert(error) })
			},
			refreshApps() {
				this.selectedApps = []
				this.getApplications()
			},
		}
    }
</script>