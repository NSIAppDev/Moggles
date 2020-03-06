<template>
  <div>
    <alert v-if="showRefreshAlert" type="info">
      <button type="button" class="close" @click="closeRefreshAlert">
        <span>×</span>
      </button>
      <h4>Toggles Have Been Modified, would you like to refresh the environments?</h4>
      <span v-for="(env, index) in environmentsToRefresh" :key="env" class="env-button">
        <button class="btn btn-default text-uppercase" @click="refreshEnvironmentToggles(env, index)"><strong>{{ env }}</strong></button>
      </span>
    </alert>
    <vue-good-table ref="toggleGrid"
                    :columns="gridColumns"
                    :rows="toggles"
                    :pagination-options="getPaginationOptions"
                    :sort-options="{
                      enabled: true,
                      initialSortBy: {field: 'toggleName', type: 'asc'}
                    }"
                    style-class="vgt-table striped condensed bordered"
                    @on-per-page-change="onPageChange">
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
          <a @click="openEditFeatureToggleModal(props.row)"><i class="fas fa-edit" /></a>
          <a v-if="!props.row.isPermanent" @click="openDeleteConfirmationModal(props.row)"><i class="fas fa-trash-alt" /></a>
          <span v-if="props.row.isPermanent" title="Permanent flags cannot be deleted!" class="disabled-link"><i class="fas fa-trash-alt" /></span>
        </span>
        <span v-else-if="props.column.field == 'toggleName' ">
          <span>{{ props.row.toggleName }}</span> <span v-if="props.row.isPermanent" class="label label-danger">Permanent</span>
          <a v-for="ft in getSchedulesForToggle(props.row.toggleName)" :key="ft.scheduleId" @click="editToggleSchedule(ft)"><i class="fas fa-clock" /> <i /></a>
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
        <a v-if="isEnvironmentColumn(props.column)" @click="openEditEnvironmentModal(props.column)"><i class="fas fa-edit" /></a>
      </template>
    </vue-good-table>

    <modal v-model="showDeleteConfirmationModal" title="You are about to delete a feature toggle" :footer="false">
      <delete-featureToggle />
    </modal>
    <modal v-model="showSchedulerModal" title="Schedule Toggles" :footer="false">
      <toggle-scheduler />
    </modal>
    <modal v-model="showEditModal" title="Edit Feature Flags" :footer="false">
      <edit-featureToggle />
    </modal>
    <modal v-model="showEditEnvironmentModal" title="Edit Environment" :footer="false">
      <edit-environment />
    </modal>
  </div>
</template>
<script>
    import axios from 'axios';
    import _ from 'lodash';
    import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
    import { Bus } from './event-bus';
    import PrettyCheck from 'pretty-checkbox-vue/check';
    import ToggleScheduler from "./ToggleScheduler";
    import EditFeatureToggle from './EditFeatureToggle';
    import EditEnvironment from './EditEnvironment';
    import DeleteFeatureToggle from './DeleteFeatureToggle';

    export default {
        components: {
            'p-check': PrettyCheck,
            'toggle-scheduler': ToggleScheduler,
            'edit-featureToggle': EditFeatureToggle,
            'edit-environment': EditEnvironment,
            'delete-featureToggle': DeleteFeatureToggle
        },
        data() {
            return {
                signalRConnection: null,
                gridColumns: [],
                rowsPerPage: 10,
                selectedApp: {},
                toggles: [],
                environmentsList: [],
                scheduledToggles: [],
                environmentsToRefresh: [],
                rowToEdit: null,
                showEditEnvironmentModal: false,
                showEditModal: false,
                showDeleteConfirmationModal: false,
                showSchedulerModal: false,
                isRefreshAlertVisible: false,
                isCacheRefreshEnabled: false
            }
        },
        computed: {
            showRefreshAlert() {
                return this.environmentsToRefresh.length > 0 ? this.isRefreshAlertVisible : false;
            },
            getPaginationOptions() {
                return { enabled: true, perPage: parseInt(this.getRowsPerPage()) };
            },
            environmentsNameList() {
                return _.map(this.environmentsList, (env) => {
                    return env.envName;
                });
            }
        },
        created() {
            axios.get("/api/CacheRefresh/getCacheRefreshAvailability").then((response) => {
                this.isCacheRefreshEnabled = response.data;
            }).catch(error => window.alert(error));

            this.subscribeToBusEvents();
        },
        mounted() {
            this.createSignalRConnection();
        },
        methods: {
            subscribeToBusEvents() {
                Bus.$on("app-changed", app => {
                    if (app) {
                        this.selectedApp = app;
                        this.loadGrid();
                    }
                })

                Bus.$on("env-added", () => {
                    this.loadGrid();
                })

                Bus.$on("toggle-added", () => {
                    this.loadGridData()
                })

                Bus.$on("toggle-scheduled", () => {
                    this.getAllScheduledToggles();
                })

                Bus.$on('close-scheduler', () => {
                    this.showSchedulerModal = false;
                    this.getAllScheduledToggles();
                })

                Bus.$on('close-editEnvironment', () => {
                    this.showEditEnvironmentModal = false;
                    this.loadGrid();
                })

                Bus.$on('close-editFeatureFlag', (environmentsToRefresh, isRefreshAlertVisible) => {
                    this.showEditModal = false;
                    this.loadGrid();
                    this.environmentsToRefresh = environmentsToRefresh;
                    this.isRefreshAlertVisible = isRefreshAlertVisible;
                })

                Bus.$on('close-deleteToggle', () => {
                    this.showDeleteConfirmationModal = false;
                    this.loadGridData();
                })
            },
            createSignalRConnection() {
                this.signalRConnection = new HubConnectionBuilder().withUrl("/isDueHub").configureLogging(LogLevel.Trace).build();

                try {
                    this.signalRConnection.off('IsDue', this.signalScheduleIsDue);
                    this.signalRConnection.on('IsDue', this.signalScheduleIsDue);
                    this.signalRConnection.start();
                } catch (err) {
                    setTimeout(() => this.createSignalRConnection(), 5000);
                }
            },
            signalScheduleIsDue() {
                this.loadGridData(this.selectedApp.id);
            },
            createGridColumns() {
                this.$refs['toggleGrid'].reset();

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
                        field: 'workItemIdentifier',
                        label: 'Work Item ID',
                        sortable: true,
                        width: '140px',
                        thClass: 'sortable',
                        filterOptions: {
                            enabled: true,
                            placeholder: 'Filter Work Item ID'
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
                columns.splice(2, 0, ..._.map(this.environmentsNameList, envName => {
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
            loadGrid() {
                this.initializeGrid(this.selectedApp);
                this.loadGridData();
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

                    Bus.$emit('env-loaded', this.environmentsNameList)
                }).catch((e) => { window.alert(e) });
            },
            loadGridData() {
                this.getAllScheduledToggles(this.selectedApp.id);

                axios.get("/api/FeatureToggles", {
                    params: {
                        applicationId: this.selectedApp.id
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
                            workItemIdentifier: toggle.workItemIdentifier,
                            createdDate: new Date(toggle.createdDate),
                            reasonsToChange: toggle.reasonsToChange.reverse()
                        }

                        this.environmentsNameList.forEach(env => {
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
            getAllScheduledToggles() {
                this.scheduledToggles = [];

                axios.get("/api/toggleScheduler", {
                    params: {
                        applicationId: this.selectedApp.id
                    }
                }).then((response) => {
                    this.scheduledToggles = response.data;
                });
            },
            getSchedulesForToggle(toggleName) {
                return _.map(this.scheduledToggles.filter(ft => ft.toggleName == toggleName), sch => {
                    return {
                        scheduleId: sch.id,
                        toggleName: sch.toggleName,
                        scheduledDate: sch.scheduledDate,
                        scheduledState: sch.scheduledState,
                        updatedBy: sch.updatedBy,
                        environments: sch.environments
                    };

                });
            },
            editToggleSchedule(toggle) {
                Bus.$emit("edit-schedule", toggle.scheduleId);
                this.showSchedulerModal = true;
            },
            openEditEnvironmentModal(column) {
                var environmentFromList = this.environmentsList.find(element => element.envName == column.field);
                this.showEditEnvironmentModal = true;
                Bus.$emit('edit-environment', this.selectedApp, environmentFromList);
            },
            openEditFeatureToggleModal(row) {
                this.rowToEdit = _.clone(row)
                this.showEditModal = true;
                let toggle = {
                    appId: this.selectedApp.id,
                    rowToEdit: this.rowToEdit
                }
                Bus.$emit('open-editFeatureToggle', toggle, this.gridColumns);
            },
            openDeleteConfirmationModal(row) {
                this.showDeleteConfirmationModal = true
                let toggleToDelete = {
                    appId: this.selectedApp.id,
                    toggle: row
                }
                Bus.$emit('delete-featureToggle', toggleToDelete);
            },
            refreshEnvironmentToggles(env, index) {
                if (!this.selectedApp)
                    return;

                Bus.$emit('block-ui')

                axios.post('api/CacheRefresh', {
                    applicationId: this.selectedApp.id,
                    envName: env
                })
                    .then(() => {
                        this.environmentsToRefresh.splice(index, 1);
                        //shouldn't need the below code, but computed value doesn't register the length as 0 without it
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
            closeRefreshAlert() {
                this.isRefreshAlertVisible = false;
            },
            isEnvironmentColumn(column) {
                return (column.type == 'boolean' && column.field != 'userAccepted');
            }
        }
    }
</script>

<style>
    .list-group {
        max-height: 200px;
        margin-bottom: 10px;
        overflow: scroll;
        -webkit-overflow-scrolling: touch;
        overflow-x: hidden;
        word-break: break-word;
    }

    textarea {
        -webkit-border-radius: 5px;
        -moz-border-radius: 5px;
        border-radius: 5px;
    }
</style>
