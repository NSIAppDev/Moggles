<template>
    <div>
		<div class="form-horizontal">
			<div class="row">
				<div class="col-sm-12">
					<div v-for="error in editAppErrors" :key="error" class="text-danger margin-left-15">{{error}}</div>
				</div>
				<div class="form-group">
					<label class="col-sm-4 control-label" for="appName">Application name:</label>
					<div class="col-sm-7">
						<input type="text" class="form-control" name="appName" v-model="appName">
					</div>
				</div>
				<div class="clearfix">
					<div class="col-sm-6">
						<button type="button" class="btn btn-danger" @click="showDeleteConfirmationMessage">Delete</button>
					</div>
					<div class="col-sm-6 text-right">
						<button type="button" class="btn btn-default" @click="cancel">Cancel</button>
						<button type="button" class="btn btn-primary" @click="updateApp">Save</button>
					</div>
				</div>
			</div>
		</div>
    </div>
</template>

<script>
    import { Bus } from './event-bus'
    import axios from 'axios'

    export default {
        data() {
            return {
                selectedApp: {},
                editAppErrors: [],
                appName: ""
            }
        },
        created() {
            Bus.$on("app-changed", app => {
                if (app) {
                    this.selectedApp = app;
                    this.appName = this.selectedApp.appName;
                }
            })
        },
        methods: {
            updateApp() {
                this.editAppErrors = [];
                if (this.stringIsNullOrEmpty(this.appName)) {
                    this.editAppErrors.push("Application name cannot be empty")
                    return;
                };

                let appUpdateModel = {
                    id: this.selectedApp.id,
                    applicationName: this.appName
                }

                axios.put('/api/applications/update', appUpdateModel)
                    .then((result) => {
                        this.$emit('close-app-edit-modal');
                        Bus.$emit("new-app-added");
                    }).catch(e => {
                        window.alert(e)
                    })
            },
            cancel() {
                this.appName = this.selectedApp.appName;
                this.$emit('close-app-edit-modal');
            },
            stringIsNullOrEmpty(text) {
                return !text || /^\s*$/.test(text);
            },
            showDeleteConfirmationMessage() {
                Bus.$emit("show-app-delete-confirmation");
            }
        }
    }
</script>