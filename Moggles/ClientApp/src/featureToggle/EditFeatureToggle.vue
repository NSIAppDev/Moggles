﻿<template>
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
              <input id="editToggleNameInput" v-model="rowToEdit.toggleName" type="text"
                     class="form-control" readonly>
            </div>
          </div>
          <div>
            <label class="col-sm-4 margin-top-8 control-label">Work Item ID</label>
            <div class="col-sm-7 margin-top-8">
              <input v-model="rowToEdit.workItemIdentifier" type="text" class="form-control">
            </div>
          </div>
          <div>
            <label class="col-sm-4 margin-top-8 control-label">Notes</label>
            <div class="col-sm-7 margin-top-8">
              <input id="editNotesInput" v-model="rowToEdit.notes" type="text"
                     class="form-control">
            </div>
          </div>
          <div class="col-sm-12 margin-top-8">
            <label class="col-sm-4 control-label">Is Permanent</label>
            <div class="col-sm-1 margin-top-10">
              <p-check id="editIsPermanentCheckbox" v-model="rowToEdit.isPermanent" class="p-icon p-fill"
                       color="default">
                <i slot="extra" class="icon fas fa-check" />
              </p-check>
            </div>
          </div>
          <div class="col-sm-12 margin-top-8">
            <label class="col-sm-4 control-label">Accepted by User</label>
            <div class="col-sm-1 margin-top-10">
              <p-check id="editAcceptedByUserCheckbox" v-model="rowToEdit.userAccepted" class="p-icon p-fill"
                       color="default">
                <i slot="extra" class="icon fas fa-check" />
              </p-check>
            </div>
          </div>
        </div>
        <div class="col-sm-12">
          <h5 class="margin-top-20">
            <strong>Environments</strong>
          </h5>
          <hr class="margin-top-1">
        </div>
        <div v-for="environment in environments" :key="environment.envName" class="form-group margin-top-8">
          <label class="col-sm-2 margin-top-8 control-label">{{ environment.envName }}</label>
          <div class="col-sm-1 margin-top-14">
            <div>
              <p-check v-model="rowToEdit[environment.envName]" class="p-icon p-fill" :color="rowToEdit[environment.envName + '_IsDeployed'] ? 'success' : 'default' ">
                <i slot="extra" class="icon fas fa-check" />
              </p-check>
            </div>
          </div>
          <div class="col-sm-8 margin-top-14">
            <div v-if="rowToEdit[environment.envName + '_FirstTimeDeployDate'] !== null">
              <strong>Deployed:</strong> {{ rowToEdit[environment.envName + '_FirstTimeDeployDate'] | moment('M/D/YY hh:mm:ss A') }}
            </div>
            <div>
              <strong>Last Updated:</strong> {{ rowToEdit[environment.envName + '_LastUpdated'] | moment('M/D/YY hh:mm:ss A') }}
            </div>
            <div v-if="rowToEdit[environment.envName + '_UpdatedByUser'] !== null">
              <strong>Updated by:</strong> {{ rowToEdit[environment.envName + '_UpdatedByUser'] }}
            </div>
          </div>
        </div>
        <div class="col-sm-12">
          <h5 class="margin-top-20">
            <strong>Change Reasons</strong>
          </h5>
          <hr class="margin-top-1">
          <textarea v-model="reasonToChange" :disabled="!featureToggleStatusesChanged" class="col-sm-12"
                    rows="2" />
          <ul class="list-group col-sm-12 margin-top-4">
            <li v-for="reason in rowToEdit.reasonsToChange" :key="reason.createdAt" class="col-sm-12 list-group-item">
              <div class="col-sm-4">
                <strong>{{ reason.addedByUser }}</strong>
                <div>{{ reason.createdAt | moment('M/D/YY hh:mm:ss A') }}</div>
                <div>
                  <span v-for="environment in reason.environments" :key="environment" class="margin-right-5"><label>{{ environment }}</label></span>
                </div>
              </div>
              <div class="col-sm-8">
                {{ reason.description }}
              </div>
            </li>
          </ul>
        </div>
        <div class="col-sm-12">
          <hr class="margin-top-1">
        </div>
      </div>
    </div>
    <div class="row">
      <div class="col-xs-6">
        <button id="deleteToggleBtnEditModal" :disabled="rowToEdit.isPermanent" type="button"
                class="btn btn-danger"	
                @click="openDeleteFeatureToggleConfirmationModal(rowToEdit)">
          Delete
        </button>
      </div>
      <div class="col-xs-6 text-right">
        <button id="cancelEditToggleBtn" type="button" class="btn btn-default"
                @click="closeModal()">
          Cancel
        </button>
        <button id="saveEditToggleBtn" type="button" class="btn btn-primary"
                @click="saveToggle">
          Save
        </button>
      </div>
    </div>

    <modal v-model="showDeleteConfirmationModal" title="You are about to delete a feature toggle" :footer="false"
           append-to-body>
      <delete-featureToggle :application="application" />
    </modal>
  </div>
</template>
<script>
	import PrettyCheck from 'pretty-checkbox-vue/check';
	import axios from 'axios';
	import { Bus } from '../common/event-bus';
	import _ from 'lodash';
	import { events } from '../common/events';
	import DeleteFeatureToggle from './DeleteFeatureToggle';

	export default {
		components: {
			'p-check': PrettyCheck,
			'delete-featureToggle': DeleteFeatureToggle
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
				editFeatureToggleErrors: [],
				showDeleteConfirmationModal: false
			}
		},
		computed: {
			featureToggleStatusesChanged() {
				let numberOfEditedEnvironments = 0;
				_.forEach(this.environments, environment => {
					if (this.environmentStatusHasChanged(environment)) {
						numberOfEditedEnvironments++;
					}
				});

				return numberOfEditedEnvironments > 0 || this.acceptedByUserHasChanged() ? true : false;
			}
		},
		created() {
			Bus.$on(events.openEditFeatureToggleModal, (toggle) => {
				this.initialiseModal();
				this.rowToEdit = toggle;
				this.initialToggle = _.cloneDeep(toggle);
                this.getEnvironments();
			});

			Bus.$on(events.closeDeleteFeatureToggleModal, () => {
				this.showDeleteConfirmationModal = false;
				this.closeModal();
			})
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
				}).catch(() => Bus.$emit(events.showErrorAlertModal, { 'customErrorMessage': 'Error getting list of environments.' }));
			},
			saveToggle() {
				this.validateEditModel();
				if (this.editModelHasErrors())
					return;

				let statuses = [];
				_.forEach(this.environments, environment => {
					if (this.environmentStatusHasChanged(environment)) {
						statuses.push({
							environment: environment.envName,
							enabled: this.rowToEdit[environment.envName]
						});
						this.addEnvironmentToRefreshList(environment.envName);
					}

				});

				let toggleUpdateModel = {
					id: this.rowToEdit.id,
					applicationid: this.application.id,
					userAccepted: this.rowToEdit.userAccepted,
					notes: this.rowToEdit.notes,
					workItemIdentifier: !this.stringIsNullOrEmpty(this.rowToEdit.workItemIdentifier) ? this.rowToEdit.workItemIdentifier : null,
					featureToggleName: this.rowToEdit.toggleName.trim(),
					isPermanent: this.rowToEdit.isPermanent,
					statuses: statuses,
					reasonToChange: !this.stringIsNullOrEmpty(this.reasonToChange) ? {
						description: this.reasonToChange,
						environments: this.environmentsToRefresh
					} : null

				}

				axios.put('/api/featuretoggles', toggleUpdateModel)
					.then(() => {
						this.closeModal();
					}).catch(error => Bus.$emit(events.showErrorAlertModal, { 'error': error }));
			},
			validateEditModel() {
				this.editFeatureToggleErrors = [];

				if (this.stringIsNullOrEmpty(this.rowToEdit.toggleName)) {
					this.editFeatureToggleErrors.push("Feature toggle name cannot be empty")
				}

				if (!this.workItemIdentifierIsValid(this.rowToEdit.workItemIdentifier)) {
					this.editFeatureToggleErrors.push("Work Item ID cannot have more than 50 characters")
				}

				if (!this.reasonToChangeIsValid(this.reasonToChange)) {
					this.editFeatureToggleErrors.push("Change reason description cannot have more than 500 characters");
				}

				_.forEach(this.environments, environment => {
					if (this.environmentStatusHasChanged(environment)) {
						if ((!this.reasonToChangeWhenToggleDisabledIsValid(environment) || !this.reasonToChangeWhenToggleEnabledIsValid(environment))) {
							this.editFeatureToggleErrors.push(`Change reason is mandatory when state is modified for environment ${environment.envName}`);
						}
					}
				});
			},
			environmentStatusHasChanged(environment) {
				return this.initialToggle[environment.envName] != this.rowToEdit[environment.envName];
            },
            acceptedByUserHasChanged() {
                return this.initialToggle.userAccepted != this.rowToEdit.userAccepted;
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
				return this.stringIsNullOrEmpty(workItemIdentifier) || (!this.stringIsNullOrEmpty(workItemIdentifier) && workItemIdentifier.length <= 50);
			},
			reasonToChangeIsValid(reasonToChange) {
				return this.stringIsNullOrEmpty(reasonToChange) || (!this.stringIsNullOrEmpty(reasonToChange) && reasonToChange.length <= 500);
			},
			editModelHasErrors() {
				return this.editFeatureToggleErrors.length > 0;
			},
			addEnvironmentToRefreshList(environment) {
				let index = _.indexOf(this.environmentsToRefresh, environment);
				if (index === -1) {
					this.environmentsToRefresh.push(environment);
				}
			},
			closeModal() {
				let isRefreshAlertVisble = this.environmentsToRefresh.length > 0;
				Bus.$emit(events.closeEditFeatureToggleModal, this.environmentsToRefresh, isRefreshAlertVisble);
			},
            openDeleteFeatureToggleConfirmationModal(row) {
				this.showDeleteConfirmationModal = true
				Bus.$emit(events.deleteFeatureToggle, row);
			},
		}
	}
</script>