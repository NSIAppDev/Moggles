<template>
    <div>
        <div class="form-horizontal">
            <div class="row">
                <div class="col-sm-12">
                    <div v-for="error in editAppErrors" :key="error" class="text-danger margin-left-15">
                        {{ error }}
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-4 control-label" for="appName">Application name:</label>
                    <div class="col-sm-7">
                        <input id="editApplicationNameInput" v-model="appName" type="text"
                               class="form-control" name="appName">
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-4 control-label" for="assignedTo">Assigned to:</label>
                    <div class="col-sm-7">
                        <select id="assignedToDropdown" v-model="assignedTo" class="form-control" name="assignedTo">
                            <option v-for="option in assignedToOptions" :key="option" :value="option">
                                {{ option }}
                            </option>
                        </select>
                    </div>
                </div>
                <div class="clearfix">
                    <div v-if="!application.isDeleted" class="col-sm-6">
                        <button id="deleteApplicationBtn" type="button" class="btn btn-danger"
                                @click="showDeleteConfirmationMessage">
                            Delete
                        </button>
                    </div>
                    <div v-else class="col-sm-6">
                        <button id="reactivateApplicationBtn" type="button" class="btn btn-primary"
                                @click="updateApp(true)">
                            Reactivate
                        </button>
                    </div>
                    <div class="col-sm-6 text-right">
                        <button type="button" class="btn btn-default" @click="cancel">
                            Cancel
                        </button>
                        <button id="saveEditApplicationBtn" type="button" class="btn btn-primary"
                                @click="updateApp(false)">
                            Save
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
    import { events } from '../common/events';

    export default {
       props: {
            application: {
                type: Object,
                required: true
            }
        },
        data() {
            return {
                editAppErrors: [],
                appName: "",
                assignedTo: "",
                assignedToOptions: []
            }
        },
        created() {
            this.appName = this.application.appName;
            this.assignedTo = this.application.assignedTo;
            axios.get('/api/applications/assignedto-options')
                .then(response => {
                    this.assignedToOptions = response.data;
                    if (!this.assignedToOptions.includes(this.assignedTo)) {
                        this.assignedTo = "";
                    }
                });
        },
        methods: {
            updateApp(reactivateButtonClicked) {
                this.editAppErrors = [];
                if (this.stringIsNullOrEmpty(this.appName)) {
                    this.editAppErrors.push("Application name cannot be empty")
                    return;
                }

                let appUpdateModel = {
                    id: this.application.id,
                    applicationName: this.appName,
                    applicationAssignedTo: this.assignedTo,
                    isDeleted: reactivateButtonClicked ? false : this.application.isDeleted
                }

                axios.put('/api/applications/update', appUpdateModel)
                    .then(() => {
                        this.$emit('close-app-edit-modal');
                        Bus.$emit(events.applicationEdited, appUpdateModel);
                        if (reactivateButtonClicked)
                            Bus.$emit(events.refreshApplications);
                    }).catch(error => Bus.$emit(events.showErrorAlertModal, { 'error': error }));
            },
            cancel() {
                this.editAppErrors = [];
                this.$emit('close-app-edit-modal');
            },
            stringIsNullOrEmpty(text) {
                return !text || /^\s*$/.test(text);
            },
            showDeleteConfirmationMessage() {
                Bus.$emit(events.showDeleteApplicationConfirmationModal);
            }
        }
    }
</script>