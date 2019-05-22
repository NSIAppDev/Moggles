<template>
    <div id="app-sel">        
		<v-select :options="applicationList" v-model="selectedAppId" @change="changeApp" options-value="id" options-label="appName"></v-select>
    </div>
</template>

<script>
	import { Bus } from './event-bus'
	import axios from 'axios'
	import { select } from 'vue-strap'
	import _ from 'lodash'

	export default {
		components: {
			'v-select': select
		},
        data() {
            return { 
				applicationList: [],
				selectedAppId: null
			}
        },
        methods: {
			changeApp(value) {
				this.selectedAppId = value
				var app = _.find(this.applicationList, (a) => a.id == this.selectedAppId)
                if (app) {
                    Bus.$emit('app-changed', app)
                }
            },
			getApplications() {
				axios.get('/api/applications')
					.then((response) => {
						this.applicationList = response.data
                        if (!this.selectedAppId) {
                            if (response.data.length > 0) {
                                this.selectedAppId = response.data[0].id;
                            }
						}
                    })
                    .catch(error => { window.alert(error) })
			}
        },
		created() {
			this.getApplications()
			Bus.$on("new-app-added", () => {
				this.getApplications
			});
			Bus.$on("reload-application-toggles", () => {
				this.changeApp(this.selectedAppId);
			});
		}
    }
</script>