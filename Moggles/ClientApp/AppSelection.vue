<template>
    <div id="app-sel"> 
		<multi-select :limit="1" :options="applicationList" v-model="selectedApps" @change="changeApp" :value-key="'id'" :label-key="'appName'" :selected-icon="'fas fa-check'" ref="appSelection"></multi-select>
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
        methods: {
			changeApp() {
				var app = _.find(this.applicationList, (a) => a.id == this.selectedApps[0])
                if (app) {
					Bus.$emit('app-changed', app)
					this.$refs.appSelection.showDropdown = false
                }
            },
			getApplications() {
				axios.get('/api/applications')
					.then((response) => {
						this.applicationList = response.data
                        if (!this.selectedAppId) {
							if (response.data.length > 0) {
								if (this.selectedApps.length == 0) {
									this.selectedApps.push(response.data[0].id);
								}
                            }
						}
                    })
                    .catch(error => { window.alert(error) })
            },
            refreshApps() {
                axios.get('/api/applications')
                    .then((response) => {
                        this.applicationList = response.data;
                        this.selectFirstApp();
                    })
                    .catch(error => { window.alert(error) })
            },
            selectFirstApp() {
                this.selectedApps = [];
                this.selectedApps.push(this.applicationList[0].id);

                var app = this.applicationList[0];
                if (app) {
                    Bus.$emit('app-changed', app)
                }
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
		}
    }
</script>