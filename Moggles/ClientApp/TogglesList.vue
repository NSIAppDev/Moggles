<template>
	<div>
		<spinner ref="spinner" v-model="spinner" size="sm"></spinner>
		<alert v-model="showSuccessAlert" placement="top-right" duration="1500" type="success" width="400px" dismissable>
			<span class="icon-ok-circled alert-icon-float-left"></span>
			<p>Cache Refreshed.</p>
		</alert>
		<alert type="info" :value="showRefreshAlert">
			<button type="button" class="close" @click="closeRefreshAlert"><span>×</span></button>
			<h4>Toggles Have Been Modified, would you like to refresh the environments?</h4>
			<span v-for="env in environmentsToRefresh" class="env-button">
				<button class="btn btn-default text-uppercase" @click="refreshEnvironment(env)"><strong>{{env}}</strong></button>
			</span>
		</alert>
		<vue-good-table ref="toggleGrid"
						:columns="gridColumns"
						:rows="toggles"
						:pagination-options="{
							enabled: true
						}"
						:sort-options="{
							enabled: true,
							initialSortBy: {field: 'toggleName', type: 'asc'}
						}"
						styleClass="vgt-table striped condensed bordered">
			<div slot="emptystate">
				<div class="text-center">There are no toggles for this application or filtered search</div>
			</div>
            <template slot="table-row" slot-scope="props">
                <span v-if="props.column.type == 'boolean'" :class="{ 'is-deployed': props.row[props.column.field + '_IsDeployed']}">
                    <checkbox v-if="props.row[props.column.field + '_IsDeployed']" v-model="props.formattedRow[props.column.field]" type="success" disabled></checkbox>
                    <checkbox v-if="!props.row[props.column.field + '_IsDeployed']" v-model="props.formattedRow[props.column.field]" disabled></checkbox>
                </span>
                <span v-else-if="props.column.field == 'id'">
                    <a @click="edit(props.row)"><i class="fas fa-edit"></i></a>
                    <a @click="confirmDelete(props.row)"><i class="fas fa-trash-alt"></i></a>
                </span>
                <span v-else-if="props.column.field == 'toggleName' && props.row.isPermanent">
                    <span>{{props.row.toggleName}}</span>  <span class="permanent-toggle">Permanent</span>
                </span>
                <span v-else-if="props.column.field == 'createdDate'">
                    {{props.formattedRow.createdDate | moment('M/D/YY hh:mm:ss A')}}
                </span>
                <span v-else>
                    {{props.formattedRow[props.column.field]}}
                </span>
            </template>
		</vue-good-table>
        <modal v-model="showEditModal">
            <div slot="modal-header" class="modal-header">
                <h4 class="modal-title">Edit Feature Flags</h4>
            </div>
            <div slot="modal-body" class="mocdal-body">
                <div v-if="rowToEdit" class="form-horizontal">
                    <div class="col-sm-8">
                        <div v-for="error in editFeatureToggleErrors" :key="error" class="validationMessage margin-left-15">{{error}}</div>
                    </div>
                    <div class="form-group" v-for="col in gridColumns">
                        <div v-if="col.type == 'boolean'">
                            <label class="col-sm-4 control-label">{{col.label}}</label>
                            <div class="col-sm-1 margin-top-8">
                                <div class="checkbox" @click="environmentEdited(col.field)">
                                    <checkbox v-if="rowToEdit[col.field + '_IsDeployed']" v-model="rowToEdit[col.field]" type="success"></checkbox>
                                    <checkbox v-if="!rowToEdit[col.field + '_IsDeployed']" v-model="rowToEdit[col.field]"></checkbox>
                                </div>

                            </div>
                            <div class="col-sm-6 margin-top-8">                      
                                <span v-if="isEnviroment(col.field) && rowToEdit[col.field + '_FirstTimeDeployDate'] !== null"><strong>Deployed:</strong> {{rowToEdit[col.field + '_FirstTimeDeployDate'] | moment('MM/DD/YYYY hh:mm')}}</span>
                                <span v-if="isEnviroment(col.field)"><strong>Last Updated:</strong> {{rowToEdit[col.field + '_LastUpdated'] | moment('MM/DD/YYYY hh:mm')}}</span>
                            </div>
                        </div>
                        <div v-else-if="col.field !== 'id' && col.field !== 'createdDate'">
                            <div class="form-group">
                                <label class="col-sm-4 control-label">{{col.label}}</label>
                                <div class="col-sm-7">
                                    <input type="text" class="form-control" v-model="rowToEdit[col.field]">
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div slot="modal-footer" class="modal-footer">
                <button type="button" class="btn btn-default" @click="cancelEdit">Cancel</button>
                <button type="button" class="btn btn-primary" @click="saveToggle">Save</button>
            </div>
        </modal>
		<modal v-model="showDeleteConfirmation" ok-text="Delete" cancel-text="Cancel" :callback="deleteToggle">
			<div slot="modal-header" class="modal-header">
				<h4 class="modal-title">You are about to delete a feature toggle</h4>
			</div>
			<div slot="modal-body" class="modal-body">
				<div v-if="toggleIsDeployed">
					<strong>{{rowDataToDelete ? rowDataToDelete.toggleName: ""}}</strong> feature toggle is active on at least one environment. Are you sure you want to delete it?
				</div>
				<div v-else>
					Are you sure you want to delete this feature toggle?
				</div>
			</div>
		</modal>
	</div>
</template>
<script>
	import { modal, checkbox, alert, spinner } from 'vue-strap'
	import axios from 'axios'
	import _ from 'lodash'
	import { Bus } from './event-bus'
	export default {
		environmentsList: [],
		components: {
			modal,
			checkbox,
			alert,
			spinner
		},
		data() {
			const PAGE_SIZE = 15;

			return {
				toggles: [],
				gridColumns: [],
				selectedApp: {},
				rowToEdit: null,
				showEditModal: false,
				showAcceptedFeatures: false,
				showDeleteConfirmation: false,
				rowDataToDelete: null,
				toggleIsDeployed: false,
				environmentsEdited: [],
				environmentsToRefresh: [],
				refreshAlertVisible: false,
                showSuccessAlert: false,
                spinner: false,
                isCacheRefreshEnabled: false,
                editFeatureToggleErrors: []
			}
		},
		created() {
            axios.get("/api/CacheRefresh/getCacheRefreshAvailability").then((response) => {
                this.isCacheRefreshEnabled = response.data;
            }).catch(error => window.alert(error));
            Bus.$on("app-changed", app => {
                if (app) {
                    this.selectedApp = app;
                    this.initializeGrid(app)
                }
			})

			Bus.$on("env-added", () => {
				this.initializeGrid(this.selectedApp);
			})

			Bus.$on("toggle-added", () => {
				this.loadGridData(this.selectedApp.id)
			})
		},
		methods: {
            saveToggle() {
                this.editFeatureToggleErrors = [];
                if (this.stringIsNullOrEmpty(this.rowToEdit.toggleName)) {
                    this.editFeatureToggleErrors.push("Feature toggle name cannot be empty")
                    return;
                };

				let toggleUpdateModel = {
					id: this.rowToEdit.id,
					userAccepted: this.rowToEdit.userAccepted,
                    notes: this.rowToEdit.notes,
                    featureToggleName: this.rowToEdit.toggleName,
                    isPermanent: this.rowToEdit.isPermanent,
					statuses: []
				}
				_.forEach(this.environmentsList, envName => {
					toggleUpdateModel.statuses.push({
						environment: envName,
						enabled: this.rowToEdit[envName]
					});
                });
				if (this.isCacheRefreshEnabled) {
					_.forEach(this.environmentsEdited, envName => {
						this.addEnvironemntToRefreshList(envName);
					});
				}
				axios.put('/api/featuretoggles', toggleUpdateModel)
					.then((result) => {
						this.showEditModal = false
						this.rowToEdit = null
						this.loadGridData(this.selectedApp.id)
						this.environmentsEdited = [];
					}).catch(error => window.alert(error))
			},
			cancelEdit() {
				this.showEditModal = false
				this.rowToEdit = null
				this.environmentsEdited = [];
			},
			createGridColumns() {
				let columns = [
					{
						field: 'id',
						label: '',
						sortable: false,
						thClass: 'width-55'
					},
					{
						field: 'toggleName',
						label: 'Feature Toggle Name',
						sortable: true,
                        thClass: 'sortable',
						filterOptions: {
							enabled: true,
							placeholder: 'Filter'
						}
					},
					{
						field: 'notes',
						label: 'Notes',
						sortable: true,
						thClass: 'sortable',
						filterOptions: {
							enabled: true,
							placeholder: 'Filter'
						}
                    },
                    {
                        field: 'isPermanent',
                        label: 'Is Permanent',
                        type: 'boolean',
                        sortable: true,
                        thClass: 'sortable',
                        filterOptions: {
                            enabled: true,
                            placeholder: 'Filter'
                        },
                        hidden: true
                    },
					{
						field: 'userAccepted',
						label: 'Accepted by User',
						type: 'boolean',
						sortable: false,
						filterOptions: {
							enabled: true,
							filterDropdownItems: [
								{ value: 'true', text: 'Accepted' },
								{ value: 'false', text: 'Unaccepted' }
							],
							filterValue: 'false',
							placeholder: 'Filter'
						}
					},
					{
						field: 'createdDate',
						label: 'Created',
						sortable: false,
					},
				]

				//create the environment columns
				columns.splice(2, 0, ..._.map(this.environmentsList, envName => {
					return {
						field: envName,
						label: envName,
						type: 'boolean',
						sortable: false,
						filterOptions: {
							enabled: true,
							filterDropdownItems: [
								{ value: 'true', text: 'Active' },
								{ value: 'false', text: 'Inactive' }
							],
							placeholder: 'Filter'
						},
					}
				}));


				this.gridColumns = columns
			},
			edit(row) {
				this.rowToEdit = row
                this.showEditModal = true
			},
			confirmDelete(row) {
				this.rowDataToDelete = row
				this.toggleIsDeployed = this.isToggleActive(this.rowDataToDelete)
				this.showDeleteConfirmation = true
			},
			deleteToggle() {
				axios.delete(`/api/FeatureToggles?id=${this.rowDataToDelete.id}`).then((result) => {
					this.showDeleteConfirmation = false
					this.rowDataToDelete = null
					this.toggleIsDeployed = false
					this.loadGridData(this.selectedApp.id)
				}).catch(error => window.alert(error))
			},
			isToggleActive(toggleData) {
				for (var propertyName in toggleData) {
					if (propertyName.endsWith("_IsDeployed") && toggleData[propertyName] === true)
						return true;
				}
				return false;
			},

			loadGridData(appId) {
				axios.get("/api/FeatureToggles", {
					params: {
						applicationId: appId
					}
				}).then((response) => {

					//create the flattened row models
					let gridRowModels = _.map(response.data, toggle => {
						let rowModel = {
							id: toggle.id,
							toggleName: toggle.toggleName,
                            userAccepted: toggle.userAccepted,
                            isPermanent: toggle.isPermanent,
							notes: toggle.notes,
							createdDate: new Date(toggle.createdDate)
						}

						this.environmentsList.forEach(env => {
							let envStatus = _.find(toggle.statuses, status => status.environment === env)
							rowModel[env] = envStatus ? envStatus.enabled : false;
                            rowModel[env + '_IsDeployed'] = envStatus ? envStatus.isDeployed : false;
                            rowModel[env + '_FirstTimeDeployDate'] = envStatus ? envStatus.firstTimeDeployDate : "";
                            rowModel[env + '_LastUpdated'] = envStatus ? envStatus.lastUpdated : "";
						});
						return rowModel;
					});

					this.toggles = gridRowModels;
					Bus.$emit('toggles-loaded', gridRowModels);

				}).catch(error => {
					//do not uncomment this, the null reference exception will return to haunt us !
					//window.alert(error)
				});
			},
			initializeGrid(app) {
				this.environmentsList = [];

				axios.get("/api/FeatureToggles/environments", {
					params: {
						applicationId: app.id
					}
				}).then((response) => {
					this.environmentsList = response.data;

					this.createGridColumns();
					this.loadGridData(app.id);
					this.$refs['toggleGrid'].reset()

                        Bus.$emit('env-loaded', response.data)
                    }).catch((e) => { window.alert(e) });
			},
			environmentEdited(env) {
				let index = _.indexOf(this.environmentsEdited, env);
				if (index === -1) {
					this.environmentsEdited.push(env);
				}
			},
			refreshEnvironment(env) {
				if (!this.selectedApp)
                    return;

				let param = {
					applicationId: this.selectedApp.id,
                    envName: env
                };

                this.spinner = true;
                axios.post('api/CacheRefresh', param)
                    .then((response) => {
                        this.spinner = false;
						this.showSuccessAlert = true;
						_.remove(this.environmentsToRefresh, function (e) {
							return e == env;
						});
						///shouldn't need the below code, but computed value doesn't register the length as 0 without it
						if (this.environmentsToRefresh.length === 0) {
							this.environmentsToRefresh = [];
						}
                    }).catch((e) => {
                        window.alert(e);
                    }).finally(() => {
                        this.spinner = false;
                    });
			},
			addEnvironemntToRefreshList(env) {
                let index = _.indexOf(this.environmentsToRefresh, env);
                if (index === -1 && this.isEnviroment(env)) {
					this.environmentsToRefresh.push(env);
					this.refreshAlertVisible = true;
				}
			},
			closeRefreshAlert() {
				this.refreshAlertVisible = false;
            },
            stringIsNullOrEmpty(text) {
                return !text || /^\s*$/.test(text);
            },
            isEnviroment(env) {
                return this.environmentsList.includes(env);
            }
		},
		computed: {
			showRefreshAlert() {
				return this.environmentsToRefresh.length > 0 ? this.refreshAlertVisible : false;
            }
		}
	}
</script>
<style>
	.width-55 {
		width: 55px !important;
	}

	th.sortable, a {
		cursor: pointer;
	}

	.success > .dropdown-toggle {
		color: #fff;
		background-color: lightgreen !important;
		border-color: #398439 !important;
	}

		.success > .dropdown-toggle.btn-success {
			color: #fff;
			background-color: #449d44 !important;
			border-color: #398439 !important;
		}

	label.checkbox > .icon {
		top: -.5rem !important;
	}

	.margin-top-8 {
		margin-top: 8px;
	}

	.env-button {
		margin-right: 10px;
	}

    .margin-left-15 {
        margin-left: 15px;
    }

    .permanent-toggle {
        margin-left: 10px;
        font-size: small;
        color: white;
        background-color: red;
        padding: 3pt 6pt
    }
</style>