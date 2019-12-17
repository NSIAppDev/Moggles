<template>
    <div>
        <alert v-if="showRefreshAlert" type="info">
            <button type="button" class="close" @click="closeRefreshAlert">
                <span>×</span>
            </button>
            <h4>Toggles Have Been Modified, would you like to refresh the environments?</h4>
            <span v-for="(env, index) in environmentsToRefresh" :key="env" class="env-button">
                <button class="btn btn-default text-uppercase" @click="refreshEnvironment(env, index)"><strong>{{ env }}</strong></button>
            </span>
        </alert>
        <vue-good-table ref="toggleGrid"
                        :columns="gridColumns"
                        :rows="toggles"
                        @on-per-page-change="onPageChange"
                        :pagination-options="getPaginationOptions"
                        :sort-options="{
                      enabled: true,
                      initialSortBy: {field: 'toggleName', type: 'asc'}
                    }"
                        style-class="vgt-table striped condensed bordered">
            <div slot="emptystate">
                <div class="text-center">
                    There are no toggles for this application or filtered search
                </div>
            </div>
            <template slot="table-row" slot-scope="props">
                <span v-if="props.column.type == 'boolean'" class="pull-left" :class="{ 'is-deployed': props.row[props.column.field + '_IsDeployed']}">
                    <p-check v-if="props.row[props.column.field + '_IsDeployed']" v-model="props.formattedRow[props.column.field]" class="p-icon p-fill p-locked"
                             color="success">
                        <i slot="extra" class="icon fas fa-check" />
                    </p-check>
                    <p-check v-if="!props.row[props.column.field + '_IsDeployed']" v-model="props.formattedRow[props.column.field]" class="p-icon p-fill p-locked"
                             color="default">
                        <i slot="extra" class="icon fas fa-check" />
                    </p-check>
                </span>
                <span v-else-if="props.column.field == 'id'">
                    <a @click="edit(props.row)"><i class="fas fa-edit" /></a>
                    <a v-if="!props.row.isPermanent" @click="confirmDelete(props.row)"><i class="fas fa-trash-alt" /></a>
                    <span v-if="props.row.isPermanent" title="Permanent flags cannot be deleted!" class="disabled-link"><i class="fas fa-trash-alt" /></span>
                </span>
                <span v-else-if="props.column.field == 'toggleName' ">
                    <span>{{ props.row.toggleName }}</span> <span v-if="props.row.isPermanent" class="label label-danger">Permanent</span>
                    <a v-for="ft in getScheduled(props.row.toggleName)" v-bind:key="ft.id" @click="editToggleScheduler(ft)"><i class="fas fa-clock" /> <i></i></a>
                </span>
                <span v-else-if="props.column.field == 'createdDate'">
                    {{ props.formattedRow.createdDate | moment('M/D/YY hh:mm:ss A') }}
                </span>
                <span v-else>
                    {{ props.formattedRow[props.column.field] }}
                </span>
            </template>
            <template slot="table-column" slot-scope="props">
                {{ props.column.label }}
                <a v-if="isEnvironmentColumn(props.column)" @click="editEnvName(props.column)"><i class="fas fa-edit" /></a>
            </template>
        </vue-good-table>

        <modal  v-model="showEditModal" title="Edit Feature Flags" :footer="false">
            <div v-if="rowToEdit" class="form-horizontal">
                <div class="row">
                    <div class="col-sm-12">
                        <div v-for="error in editFeatureToggleErrors" :key="error" class="text-danger margin-left-15">
                            {{ error }}
                        </div>
                    </div>
                    <div v-for="col in gridColumns" :key="col.field" class="form-group">
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
        </modal>
        <modal v-model="showDeleteConfirmation" title="You are about to delete a feature toggle" :footer="false">
            <div v-if="toggleIsDeployed">
                <strong>{{ rowDataToDelete ? rowDataToDelete.toggleName: "" }}</strong> feature toggle is active on at least one environment. Are you sure you want to delete it?
            </div>
            <div v-else>
                Are you sure you want to delete this feature toggle?
            </div>
            <div class="text-right">
                <button type="button" class="btn btn-default" @click="showDeleteConfirmation = false">
                    Cancel
                </button>
                <button type="button" class="btn btn-primary" @click="deleteToggle">
                    Delete
                </button>
            </div>
        </modal>

        <modal v-model="showEditEnvironmentModal" title="Edit Environment" :footer="false">
            <div v-if="environmentToEdit" class="form-horizontal">
                <div class="row">
                    <div class="col-sm-12">
                        <div v-for="error in editEnvErrors" :key="error" class="text-danger margin-left-15">
                            {{ error }}
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="col-sm-4 control-label">Environment name</label>
                        <div class="col-sm-7">
                            <input v-model="editedEnvironmentName" type="text" class="form-control">
                        </div>
                    </div>
                    <div class="clearfix">
                        <div class="col-sm-6">
                            <button type="button" class="btn btn-danger" @click="confirmDeleteEnvironment">
                                Delete
                            </button>
                        </div>
                        <div class="col-sm-6 text-right">
                            <button type="button" class="btn btn-default" @click="cancelEditEnvName">
                                Cancel
                            </button>
                            <button type="button" class="btn btn-primary" :disabled="!enableEditEnvironmentSave"
                                    @click="saveEnvironment">
                                Save
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </modal>
        <modal v-model="showDeleteEnvironmentConfirmation" title="You are about to delete an environment" :footer="false">
            <div>
                Are you sure you want to delete the environment?
                <br>
                All associated applications and feature toggles will be removed.
            </div>
            <div class="text-right">
                <button type="button" class="btn btn-default" @click="showDeleteEnvironmentConfirmation = false">
                    Cancel
                </button>
                <button type="button" class="btn btn-primary" @click="deleteEnvironment">
                    Delete
                </button>
            </div>
        </modal>
        <modal v-model="showScheduler" title="Schedule Toggles" :footer="false">
            <toggle-scheduler />
        </modal>
    </div>
</template>
<script>
	import PrettyCheck from 'pretty-checkbox-vue/check';
    import axios from 'axios';
    import _ from 'lodash';
    import { Bus } from './event-bus';
    import ToggleScheduler from "./ToggleScheduler";
    import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
	export default {
        environmentsList: [],
		components: {
            'p-check': PrettyCheck,
            'toggle-scheduler': ToggleScheduler
		},
		data() {

            return {              
				toggles: [],
				gridColumns: [],
				selectedApp: {},
                rowToEdit: null,
                environmentToEdit: null,
                showEditEnvironmentModal: false,
                showDeleteEnvironmentConfirmation: false,
                showEditModal: false,   
				showAcceptedFeatures: false,
				showDeleteConfirmation: false,
				rowDataToDelete: null,
				toggleIsDeployed: false,
				environmentsEdited: [],
				environmentsToRefresh: [],
				refreshAlertVisible: false,
				isCacheRefreshEnabled: false,
                editFeatureToggleErrors: [],
                editEnvErrors: [],
                editedEnvironmentName: "",
                editFeatureToggleScheduler: [],
                scheduledToggles: [],
                showScheduler: false,
                connectionId: null,
                connection: null,
                rowsPerPage: 10
            }
		},
		computed: {
			showRefreshAlert() {
				return this.environmentsToRefresh.length > 0 ? this.refreshAlertVisible : false;
            },
            enableEditEnvironmentSave() {
                return this.environmentToEdit.initialEnvName !== this.editedEnvironmentName;
            },
            getPaginationOptions() {
                return { enabled: true, perPage: parseInt(this.getRowsPerPage()) };
            }
		},
        created() {
            this.connection = new HubConnectionBuilder().withUrl("/isDueHub").configureLogging(LogLevel.Trace).build();
           
            axios.get("/api/CacheRefresh/getCacheRefreshAvailability").then((response) => {
                this.isCacheRefreshEnabled = response.data;
            }).catch(error => window.alert(error));
            Bus.$on("app-changed", app => {
                if (app) {
                    this.scheduledToggles = [];
                    this.selectedApp = app;
                    this.initializeGrid(app);
                    this.getAllScheduledToggle(this.selectedApp.id);
                }
            })

            Bus.$on("env-added", () => {
                this.initializeGrid(this.selectedApp);
            })

            Bus.$on("toggle-added", () => {
                this.loadGridData(this.selectedApp.id)
            })

            Bus.$on("toggle-scheduled", () => {
                this.getAllScheduledToggle(this.selectedApp.id);
            })
            
            Bus.$on('close-scheduler', () => {
                this.showScheduler = false;
            })
            

        },
        mounted() {
            this.start();
        },
        methods: {
            getRowsPerPage() {
                if (localStorage.getItem('rowsPerPage') != null) {
                    this.rowsPerPage = localStorage.getItem('rowsPerPage');
                }
                return this.rowsPerPage;
            },
            onPageChange(page) {
                let perPage = page.currentPerPage;
                this.rowsPerPage = perPage;
                localStorage.setItem('rowsPerPage', perPage);
            },
            start() {
                try {
                    this.connection.off('IsDue', this.signal);
                    this.connection.on('IsDue', this.signal);
                    this.connection.start();
                } catch (err) {
                    setTimeout(() => this.start, 5000);
                }
            },
            signal() {
                                    this.scheduledToggles = [];
                this.loadGridData(this.selectedApp.id);
                this.getAllScheduledToggle(this.selectedApp.id);
            },
            saveEnvironment() {
                this.editEnvErrors = []
                if (this.stringIsNullOrEmpty(this.editedEnvironmentName)) {
                    this.editEnvErrors.push("Environment name cannot be empty")
                    return;
                }

                let envUpdateModel = {
                    applicationId: this.selectedApp.id,
                    initialEnvName: this.environmentToEdit.initialEnvName,
                    newEnvName: this.editedEnvironmentName
                }

                axios.put('/api/FeatureToggles/updateenvironment', envUpdateModel)
					.then(() => {
						this.showEditEnvironmentModal = false
                        this.environmentToEdit = null
                        this.initializeGrid(this.selectedApp);
					}).catch(error => window.alert(error))
            },
			saveToggle() {
				this.editFeatureToggleErrors = [];
				if (this.stringIsNullOrEmpty(this.rowToEdit.toggleName)) {
					this.editFeatureToggleErrors.push("Feature toggle name cannot be empty")
					return;
				}

				let toggleUpdateModel = {
                    id: this.rowToEdit.id,
                    applicationid: this.selectedApp.id,
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
                    .then(() => {
                        this.showEditModal = false
                        this.rowToEdit = null
                        this.loadGridData(this.selectedApp.id)
                        this.environmentsEdited = []
                        Bus.$emit("app-changed", this.selectedApp)
                    }).catch(error => window.alert(error))
            },
            cancelEditEnvName() {
                this.showEditEnvironmentModal = false
                this.environmentToEdit = null
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
							placeholder: 'Filter Toggle Name'
						}
					},
					{
						field: 'notes',
						label: 'Notes',
						sortable: true,
						thClass: 'sortable',
						filterOptions: {
							enabled: true,
							placeholder: 'Filter Notes'
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
							placeholder: 'All'
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
							placeholder: 'All'
						},
					}
				}));


				this.gridColumns = columns
            },
            editEnvName(column) {      
                this.environmentToEdit = {}
                var environmentFromList = this.environmentsList.find(element => element == column.field)
                this.environmentToEdit.initialEnvName = environmentFromList
                this.editedEnvironmentName = environmentFromList
                this.showEditEnvironmentModal = true               
            },
            confirmDeleteEnvironment() {
                this.showDeleteEnvironmentConfirmation = true
            },
            deleteEnvironment() {
                let environmentModel = {
                    applicationId: this.selectedApp.id,
                    envName: this.environmentToEdit.initialEnvName
                }

                axios.delete(`/api/FeatureToggles/environments`, { data: environmentModel }).then(() => {
                    this.showDeleteEnvironmentConfirmation = false
                    this.showEditEnvironmentModal = false
                    this.environmentToEdit = null
                    this.initializeGrid(this.selectedApp);
                }).catch(error => window.alert(error))
            },
            edit(row) {
				this.rowToEdit = _.clone(row)
				this.showEditModal = true
			},
			confirmDelete(row) {
				this.rowDataToDelete = row
				this.toggleIsDeployed = this.isToggleActive(this.rowDataToDelete)
				this.showDeleteConfirmation = true
            },
            deleteToggle() {
                axios.delete(`/api/FeatureToggles?id=${this.rowDataToDelete.id}&applicationid=${this.selectedApp.id}`).then(() => {
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
            getAllScheduledToggle(appId) {

                axios.get("/api/toggleScheduler", {
                    params: {
                        applicationId: appId
                    }
                }).then((response) => {

                    //create the flattened row models
                    let models = _.map(response.data, toggle => {
                        return  {
                            id: toggle.id,
                            toggleName: toggle.toggleName,
                            scheduledDate: toggle.scheduledDate,
                            scheduledState: toggle.scheduledState,
                            updatedBy: toggle.updatedBy,
                            environments: toggle.environments
                        };
                    });
                    this.scheduledToggles = models;
                });
            },

            getScheduled(toggleName) {
                let filtered = _.map(this.scheduledToggles.filter(ft => ft.toggleName == toggleName), sch => {
                    return  {
                        toggleId: sch.id,
                        toggleName: sch.toggleName,
                        scheduledDate: sch.scheduledDate,
                        scheduledState: sch.scheduledState,
                        updatedBy: sch.updatedBy,
                        environments: sch.environments
                    };
                    
                });
                return filtered;
            },
            editToggleScheduler(toggle) {
                Bus.$emit("edit-schedule", toggle.toggleId);
                this.showScheduler = true;            
            },


            loadGridData(appId) {
                this.getAllScheduledToggle(appId);
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
                            rowModel[env + '_UpdatedByUser'] = envStatus ? envStatus.updatedByUser : "";
                        });
						return rowModel;
					});
                    this.toggles = gridRowModels;
					Bus.$emit('toggles-loaded', gridRowModels);
				}).catch(() => {
					//do not uncomment this, the null reference exception will return to haunt us !
					//window.alert(error)
				});
			},
			initializeGrid(app) {
                this.environmentsList = [];
                this.getAllScheduledToggle(app.id);
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
			refreshEnvironment(env, index) {
				if (!this.selectedApp)
					return;

				let param = {
					applicationId: this.selectedApp.id,
					envName: env
				};

				Bus.$emit('block-ui')
				axios.post('api/CacheRefresh', param)
					.then(() => {
						this.environmentsToRefresh.splice(index, 1);
						///shouldn't need the below code, but computed value doesn't register the length as 0 without it
						if (this.environmentsToRefresh.length === 0) {
							this.environmentsToRefresh = [];
						}
						this.$notify({
							type: 'success',
							content: `${env} Cache Refreshed.`,
							offsetY: 70,
							icon: 'fas fa-check-circle'
						})
					}).catch((e) => {
						window.alert(e);
					}).finally(() => {
						Bus.$emit('unblock-ui')
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
            },
            isEnvironmentColumn(column) {
                return (column.type == 'boolean' && column.field != 'userAccepted');
            }
		}
	}
</script>