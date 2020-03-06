<template>
  <div>
    <div v-if="rowToEdit" class="form-horizontal">
      <div class="row">
        <div class="col-sm-12">
          <div v-for="error in editFeatureToggleErrors" :key="error" class="text-danger margin-left-15">
            {{ error }}
          </div>
        </div>
        <div v-for="col in gridColumns" :key="col.field" class="form-group margin-bottom-4">
          <div v-if="col.type == 'boolean'">
            <label class="col-sm-4 control-label">{{ col.label }}</label>
            <div class="col-sm-1 margin-top-8">
              <div @click="environmentEdited(col.field)">
                <p-check v-if="rowToEdit[col.field + '_IsDeployed']" v-model="rowToEdit[col.field]" class="p-icon p-fill"
                         color="success">
                  <i slot="extra" class="icon fas fa-check" />
                </p-check>
                <p-check v-if="!rowToEdit[col.field + '_IsDeployed']" v-model="rowToEdit[col.field]" class="p-icon p-fill"
                         color="default">
                  <i slot="extra" class="icon fas fa-check" />
                </p-check>
              </div>
            </div>
            <div class="col-sm-6 margin-top-8">
              <div v-if="isEnviroment(col.field) && rowToEdit[col.field + '_FirstTimeDeployDate'] !== null">
                <strong>Deployed:</strong> {{ rowToEdit[col.field + '_FirstTimeDeployDate'] | moment('M/D/YY hh:mm:ss A') }}
              </div>
              <div v-if="isEnviroment(col.field)">
                <strong>Last Updated:</strong> {{ rowToEdit[col.field + '_LastUpdated'] | moment('M/D/YY hh:mm:ss A') }}
              </div>
              <div v-if="isEnviroment(col.field)">
                <strong>Updated by:</strong> {{ rowToEdit[col.field + '_UpdatedByUser'] }}
              </div>
            </div>
          </div>
          <div v-else-if="col.field !== 'id' && col.field !== 'createdDate'">
            <div class="form-group">
              <label class="col-sm-4 control-label">{{ col.label }}</label>
              <div class="col-sm-7">
                <input v-model="rowToEdit[col.field]" type="text" class="form-control">
              </div>
            </div>
          </div>
        </div>
        <div class="col-sm-12 margin-top-0">
          <label class="control-label margin-top-4">Change reason:</label>
          <textarea v-model="reasonToChange" class="col-sm-12" rows="2" />
          <ul class="list-group col-sm-12 margin-top-6">
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
      <button type="button" class="btn btn-default" @click="cancelEdit">
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
        data() {
            return {
                editFeatureToggleErrors: [],
                reasonsToChange: [],
                rowToEdit: null,
                showEditModal: false,
                requireReasonWhenToggleEnabled: false,
                requireReasonWhenToggleDisabled: false,
                reasonToChange: "",
                toggleStatuses: [],
                gridColumns: [],
                selectedAppId: null,
                environmentsEdited: [],
                environmentList: [],
                environmentsNameList: [],
                refreshAlertVisible: false,
                isCacheRefreshEnabled: false,
                environmentsToRefresh: []
            }
        },
        created() {
            axios.get("/api/CacheRefresh/getCacheRefreshAvailability").then((response) => {
                this.isCacheRefreshEnabled = response.data;
            }).catch(error => window.alert(error));

            Bus.$on("open-editFeatureToggle", (toggle, gridColumns) => {
                this.editFeatureToggleErrors = [];
                this.selectedAppId = toggle.appId;
                this.gridColumns = gridColumns;
                this.rowToEdit = toggle.rowToEdit;
                this.loadToggleStatuses();
                this.loadEnvironmentsList();
            });

        },
        methods: {
            saveToggle() {
                this.editFeatureToggleErrors = [];
                this.reasonsToChange = [];
                if (this.stringIsNullOrEmpty(this.rowToEdit.toggleName)) {
                    this.editFeatureToggleErrors.push("Feature toggle name cannot be empty")
                    return;
                }

                let workItemIdentifier = this.rowToEdit.workItemIdentifier != null ? this.rowToEdit.workItemIdentifier.trim() : this.rowToEdit.workItemIdentifier;

                if (!this.workItemIdentifierIsValid(workItemIdentifier)) {
                    this.editFeatureToggleErrors.push("Work Item ID cannot have more than 50 characters")
                    return;
                }

                if (this.reasonToChange.length > 500) {
                    this.editFeatureToggleErrors.push("Change reason description cannot have more than 500 characters");
                    return;
                }

                let toggleUpdateModel = {
                    id: this.rowToEdit.id,
                    applicationid: this.selectedAppId,
                    userAccepted: this.rowToEdit.userAccepted,
                    notes: this.rowToEdit.notes,
                    workItemIdentifier: workItemIdentifier,
                    featureToggleName: this.rowToEdit.toggleName,
                    isPermanent: this.rowToEdit.isPermanent,
                    statuses: [],
                    reasonsToChange: []
                }
                if (!this.stringIsNullOrEmpty(this.reasonToChange)) {
                    toggleUpdateModel.reasonsToChange.push({ description: this.reasonToChange });
                }

                let changes = [];
                let toggle = this.toggleStatuses.find(_ => _.id == toggleUpdateModel.id);

                _.forEach(this.environmentsNameList, envName => {
                    toggleUpdateModel.statuses.push({
                        environment: envName,
                        enabled: this.rowToEdit[envName]
                    });
                    let env = toggle.statuses.find(_ => _.environment == envName);
                    if (env.enabled != this.rowToEdit[envName]) {
                        changes.push({
                            envName: envName,
                            oldValue: env.enabled,
                            newValue: this.rowToEdit[envName]
                        });
                    }
                });

                let requiredErrorMessages = [];
                _.forEach(changes, change => {
                    let env = this.environmentsList.find(_ => _.envName == change.envName);
                    if (this.reasonToChangeWhenToggleDisabledIsValid(env, change) || this.reasonToChangeWhenToggleEnabledIsValid(env, change)) {
                        requiredErrorMessages.push(env.envName);
                    }
                });

                if (requiredErrorMessages.length > 0) {
                    _.forEach(requiredErrorMessages, status => {
                        this.editFeatureToggleErrors.push("Change reason is mandatory when state is modified for environment " + status);
                    });
                    return;
                }
                if (this.isCacheRefreshEnabled) {
                    _.forEach(this.environmentsEdited, envName => {
                        this.addEnvironemntToRefreshList(envName);
                    });
                }

                axios.put('/api/featuretoggles', toggleUpdateModel)
                    .then(() => {
                        this.showEditModal = false
                        this.rowToEdit = null
                        this.environmentsEdited = []
                        this.reasonToChange = ""
                        Bus.$emit('close-editFeatureFlag', this.environmentsToRefresh, this.refreshAlertVisible);
                    }).catch(error => window.alert(error))
            },
            stringIsNullOrEmpty(text) {
                return !text || /^\s*$/.test(text);
            },

            reasonToChangeWhenToggleDisabledIsValid(env, change) {
                return env.requireReasonWhenToggleDisabled == true && change.oldValue == true && change.newValue == false && this.stringIsNullOrEmpty(this.reasonToChange);
            },
            reasonToChangeWhenToggleEnabledIsValid(env, change) {
                return env.requireReasonWhenToggleEnabled == true && change.oldValue == false && change.newValue == true && this.stringIsNullOrEmpty(this.reasonToChange);
            },
            workItemIdentifierIsValid(workItemIdentifier) {
                return workItemIdentifier == null || (workItemIdentifier != null && workItemIdentifier.length <= 50);
            },
            environmentEdited(env) {
                let index = _.indexOf(this.environmentsEdited, env);
                if (index === -1) {
                    this.environmentsEdited.push(env);
                }
            },

            addEnvironemntToRefreshList(env) {
                let index = _.indexOf(this.environmentsToRefresh, env);
                if (index === -1 && this.isEnviroment(env)) {
                    this.environmentsToRefresh.push(env);
                    this.refreshAlertVisible = true;
                }
            },
            cancelEdit() {
                this.showEditModal = false
                this.rowToEdit = null
                this.environmentsEdited = [];
                this.editFeatureToggleErrors = []
                this.reasonToChange = "";
                Bus.$emit('close-editFeatureFlag', this.environmentsToRefresh, this.refreshAlertVisible);
            },
            isEnviroment(env) {
                return this.environmentsNameList.includes(env);
            },
            loadToggleStatuses() {
                axios.get("/api/FeatureToggles", {
                    params: {
                        applicationId: this.selectedAppId
                    }
                }).then((response) => {
                    this.toggleStatuses = response.data;
                }).catch(() => {
                    //do not uncomment this, the null reference exception will return to haunt us !
                    //window.alert(error)
                });
            },
            loadEnvironmentsList() {
                axios.get("/api/FeatureToggles/environments", {
                    params: {
                        applicationId: this.selectedAppId
                    }
                }).then((response) => {
                    this.environmentsList = response.data;
                    this.environmentsList.forEach(env => { if (!this.environmentsNameList.includes(env.envName)) { this.environmentsNameList.push(env.envName) } });

                }).catch((e) => { window.alert(e) });
            }
        }
    }
</script>