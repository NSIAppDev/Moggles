<template>
    <div>
        <div v-if="rowToEdit" class="form-horizontal">
            <div class="row">
                <div class="col-sm-12">
                    <div v-for="error in editFeatureToggleErrors" :key="error" class="text-danger margin-left-15">
                        {{ error }}
                    </div>
                </div>
                <div class="form-group">
                    <div>
                        <label class="col-sm-4 control-label">Feature Toggle name</label>
                        <div class="col-sm-7">
                            <input v-model="rowToEdit.toggleName" type="text" class="form-control">
                        </div>
                    </div>
                    <div v-for="environment in environments" :key="environment.envName" class="form-group">
                        <label class="col-sm-4 control-label">{{ environment.envName }}</label>
                        <div class="col-sm-1">
                            <div>
                                <p-check @click="addEnvironmentToRefreshList(environment.envName)" v-model="rowToEdit[environment.envName]" class="p-icon p-fill"
                                         :color="rowToEdit[environment.envName + '_IsDeployed'] ? 'success' : 'default' ">
                                    <i slot="extra" class="icon fas fa-check" />
                                </p-check>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <div v-if="rowToEdit[environment.envName + '_FirstTimeDeployDate'] !== null">
                                <strong>Deployed:</strong> {{ rowToEdit[environment.envName + '_FirstTimeDeployDate'] | moment('M/D/YY hh:mm:ss A') }}
                            </div>
                            <div>
                                <strong>Last Updated:</strong> {{ rowToEdit[environment.envName + '_LastUpdated'] | moment('M/D/YY hh:mm:ss A') }}
                            </div>
                            <div>
                                <strong>Updated by:</strong> {{ rowToEdit[environment.envName + '_UpdatedByUser'] }}
                            </div>
                        </div>
                    </div>
                    <div>
                        <label class="col-sm-4 control-label">Work Item ID</label>
                        <div class="col-sm-7">
                            <input v-model="rowToEdit.workItemIdentifier" type="text" class="form-control">
                        </div>
                    </div>
                    <div>
                        <label class="col-sm-4 control-label">Notes</label>
                        <div class="col-sm-7">
                            <input v-model="rowToEdit.notes" type="text" class="form-control">
                        </div>
                    </div>
                    <div>
                        <label class="col-sm-4 control-label">Is Permanent</label>
                        <div class="col-sm-1">
                            <p-check v-model="rowToEdit.isPermanent" class="p-icon p-fill"
                                     color="default">
                                <i slot="extra" class="icon fas fa-check" />
                            </p-check>
                        </div>
                    </div>
                    <div>
                        <label class="col-sm-4 control-label">Accepted by User</label>
                        <div class="col-sm-1">
                            <p-check v-model="rowToEdit.userAccepted" class="p-icon p-fill"
                                     color="default">
                                <i slot="extra" class="icon fas fa-check" />
                            </p-check>
                        </div>
                    </div>
                </div>
                <div class="col-sm-12">
                    <label class="control-label">Change reason:</label>
                    <textarea v-model="reasonToChange" class="col-sm-12" rows="2" />
                    <ul class="list-group col-sm-12">
                        <li v-for="reason in rowToEdit.reasonsToChange" :key="reason.createdAt" class="col-sm-12 list-group-item">
                            <div class="col-sm-4">
                                <strong>{{ reason.addedByUser }}</strong>
                                <div>{{ reason.createdAt | moment('M/D/YY hh:mm:ss A') }}</div>
                            </div>
                            <div class="col-sm-8">
                                {{ reason.description }}
                            </div>
                        </li>
                    </ul>
                </div>
            </div>
        </div>
        <div class="text-right">
            <button type="button" class="btn btn-default" @click="closeModal()">
                Cancel
            </button>
            <button type="button" class="btn btn-primary" @click="saveToggle">
                Save
            </button>
        </div>
    </div>
</template>
<script>
    import PrettyCheck from 'pretty-checkbox-vue/check';
    import axios from 'axios';
    import { Bus } from './event-bus';
    import _ from 'lodash';


    export default {
        components: {
            'p-check': PrettyCheck,
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
                rowToEdit: null,
                initialToggle: null,
                environments: [],
                reasonToChange: "",
                environmentsToRefresh: [],
                editFeatureToggleErrors: []
            }
        },
        created() {
            Bus.$on("open-editFeatureToggle", (toggle) => {
                this.initialiseModal();
                this.rowToEdit = toggle;
                this.initialToggle = _.cloneDeep(toggle);
                this.getEnvironments();
            });
        },
        methods: {
            initialiseModal() {
                this.rowToEdit = null;
                this.initialToggle = null;
                this.environments = [];
                this.reasonToChange = "";
                this.environmentsToRefresh = [];
                this.editFeatureToggleErrors = [];
            },
            getEnvironments() {
                axios.get("/api/FeatureToggles/environments", {
                    params: {
                        applicationId: this.application.id
                    }
                }).then((response) => {
                    this.environments = response.data;
                }).catch(() => {
                    window.alert("Error getting list of environments.");
                });
            },
            saveToggle() {
                this.editFeatureToggleErrors = [];

                if (this.stringIsNullOrEmpty(this.rowToEdit.toggleName)) {
                    this.editFeatureToggleErrors.push("Feature toggle name cannot be empty")
                    return;
                }

                let workItemIdentifier = this.rowToEdit.workItemIdentifier != null ? this.rowToEdit.workItemIdentifier.trim() : this.rowToEdit.workItemIdentifier;
                if (!this.workItemIdentifierIsValid(workItemIdentifier)) {
                    this.editFeatureToggleErrors.push("Work Item ID cannot have more than 50 characters")
                    return;
                }

                if (!this.reasonToChangeIsValid(this.reasonToChange)) {
                    this.editFeatureToggleErrors.push("Change reason description cannot have more than 500 characters");
                    return;
                }

                let toggleUpdateModel = {
                    id: this.rowToEdit.id,
                    applicationid: this.application.id,
                    userAccepted: this.rowToEdit.userAccepted,
                    notes: this.rowToEdit.notes,
                    workItemIdentifier: workItemIdentifier,
                    featureToggleName: this.rowToEdit.toggleName,
                    isPermanent: this.rowToEdit.isPermanent,
                    statuses: [],
                    reasonToChange: !this.stringIsNullOrEmpty(this.reasonToChange) ? this.reasonToChange : null
                }

                _.forEach(this.environments, environment => {
                    toggleUpdateModel.statuses.push({
                        environment: environment.envName,
                        enabled: this.rowToEdit[environment.envName]
                    });
                    if (this.environmentStatusHasChanged(environment) && (!this.reasonToChangeWhenToggleDisabledIsValid(environment) || !this.reasonToChangeWhenToggleEnabledIsValid(environment))) {
                        this.editFeatureToggleErrors.push(`Change reason is mandatory when state is modified for environment ${environment.envName}`);
                    }
                });

                axios.put('/api/featuretoggles', toggleUpdateModel)
                    .then(() => {
                        this.closeModal();
                    }).catch(error => window.alert(error))
            },
            environmentStatusHasChanged(environment) {
                return this.initialToggle[environment.envName] != this.rowToEdit[environment.envName];
            },
            stringIsNullOrEmpty(text) {
                return !text || /^\s*$/.test(text);
            },
            reasonToChangeWhenToggleDisabledIsValid(environment) {
                if (environment.requireReasonWhenToggleDisabled == true && this.initialToggle[environment.envName] == true && this.rowToEdit[environment.envName] == false) {
                    return !this.stringIsNullOrEmpty(this.reasonToChange);
                }
                return true;
            },
            reasonToChangeWhenToggleEnabledIsValid(environment) {
                if (environment.requireReasonWhenToggleEnabled == true && this.initialToggle[environment.envName] == false && this.rowToEdit[environment.envName] == true) {
                    return !this.stringIsNullOrEmpty(this.reasonToChange);
                }
                return true;
            },
            workItemIdentifierIsValid(workItemIdentifier) {
                return workItemIdentifier == null || (workItemIdentifier != null && workItemIdentifier.length <= 50);
            },
            reasonToChangeIsValid(reasonToChange) {
                return reasonToChange == null || (reasonToChange != null && reasonToChange.length <= 500);
            },
            addEnvironmentToRefreshList(env) {
                let index = _.indexOf(this.environmentsToRefresh, env);
                if (index === -1) {
                    this.environmentsToRefresh.push(env);
                }
            },
            closeModal() {
                let isRefreshAlertVisble = this.environmentsToRefresh.length > 0;
                Bus.$emit('close-editFeatureFlag', this.environmentsToRefresh, isRefreshAlertVisble);
            }
        }
    }
</script>