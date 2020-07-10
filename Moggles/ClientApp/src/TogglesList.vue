<template>
  <div>
    <alert v-if="showRefreshAlert" type="info">
      <button type="button" class="close" @click="closeRefreshAlert">
        <span>×</span>
      </button>
      <h4>Toggles Have Been Modified, would you like to refresh the environments?</h4>
      <span v-for="(env, index) in environmentsToRefresh" :key="env" class="env-button">
        <button id="refreshEnvironmentsBtn" class="btn btn-default text-uppercase" @click="refreshEnvironmentToggles(env, index)"><strong>{{ env }}</strong></button>
      </span>
    </alert>
    <vue-good-table id="toggleGrid" ref="toggleGrid"
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
          <a v-if="!props.row.isPermanent" @click="openDeleteFeatureToggleConfirmationModal(props.row)"><i class="fas fa-trash-alt" /></a>
          <span v-if="props.row.isPermanent" title="Permanent flags cannot be deleted!" class="disabled-link"><i class="fas fa-trash-alt" /></span>
        </span>
        <span v-else-if="props.column.field == 'toggleName' ">
          <span>{{ props.row.toggleName }}</span> <span v-if="props.row.isPermanent" class="label label-danger">Permanent</span>
          <a v-for="schedule in getSchedulesForToggle(props.row.toggleName)" :key="schedule.scheduleId" @click="editToggleSchedule(schedule)"><i class="fas fa-clock" /> <i /></a>
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
      <delete-featureToggle :application="selectedApp" />
    </modal>
    <modal v-if="showSchedulerModal" v-model="showSchedulerModal" title="Edit Feature Toggle Schedule"
           :footer="false">
      <edit-toggle-schedule :application="selectedApp" :is-cache-refresh-enabled="isCacheRefreshEnabled" :schedule="scheduleToEdit" />
    </modal>
    <modal v-model="showEditModal" title="Edit Feature Flags" :footer="false">
      <edit-featureToggle :application="selectedApp" :is-cache-refresh-enabled="isCacheRefreshEnabled" />
    </modal>
    <modal v-model="showEditEnvironmentModal" title="Edit Environment" :footer="false">
      <edit-environment :application="selectedApp" />
    </modal>
  </div>
</template>
<script>
    import axios from 'axios';
    import _ from 'lodash';
    import moment from 'moment';
    import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
    import { Bus } from './common/event-bus';
    import { events } from './common/events';
    import PrettyCheck from 'pretty-checkbox-vue/check';
    import EditToggleSchedule from "./featureToggleSchedule/EditToggleSchedule";
    import EditFeatureToggle from './featureToggle/EditFeatureToggle';
    import EditEnvironment from './environment/EditEnvironment';
    import DeleteFeatureToggle from './featureToggle/DeleteFeatureToggle';

    export default {
        components: {
            'p-check': PrettyCheck,
            'edit-toggle-schedule': EditToggleSchedule,
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
                environments: [],
                scheduledToggles: [],
                scheduleToEdit: {},
                environmentsToRefresh: [],
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
                return this.environmentsToRefresh.length > 0 && this.isCacheRefreshEnabled ? this.isRefreshAlertVisible : false;
            },
            getPaginationOptions() {
                return { enabled: true, perPage: parseInt(this.getRowsPerPage()) };
            },
            environmentsNameList() {
                return _.map(this.environments, (env) => {
                    return env.envName;
                });
            }
        },
        created() {
            axios.get("/api/CacheRefresh/getCacheRefreshAvailability").then((response) => {
                this.isCacheRefreshEnabled = response.data;
            }).catch(error => Bus.$emit(events.showErrorAlertModal, { 'error': error }));
            this.subscribeToBusEvents();
        },
        mounted() {
            this.createSignalRConnection();
        },
        methods: {
            subscribeToBusEvents() {
                Bus.$on(events.applicationChanged, app => {
                    if (app) {
                        this.selectedApp = app;
                        this.loadGrid();
                    }
                })

                Bus.$on(events.applicationEdited, applicationUpdateModel => {
                    this.selectedApp.appName = applicationUpdateModel.applicationName
                })

                Bus.$on(events.environmentAdded, () => {
                    this.loadGrid();
                })

                Bus.$on(events.toggleAdded, () => {
                    this.loadGridData()
                })

                Bus.$on(events.toggleScheduled, () => {
                    this.getAllScheduledToggles();
                })

                Bus.$on(events.closeToggleSchedulerModal, () => {
                    this.showSchedulerModal = false;
                    this.getAllScheduledToggles();
                })

                Bus.$on(events.closeEditEnvironmentModal, () => {
                    this.showEditEnvironmentModal = false;
                    this.loadGrid();
                })

                Bus.$on(events.closeEditFeatureToggleModal, (environmentsToRefresh, isRefreshAlertVisible) => {
                    this.showEditModal = false;
                    this.loadGridData();
                    this.environmentsToRefresh = environmentsToRefresh;
                    this.isRefreshAlertVisible = isRefreshAlertVisible;
                })

                Bus.$on(events.closeDeleteFeatureToggleModal, () => {
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
                this.loadGridData();
            },
            createGridColumns() {

                this.loadGridData();
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
                        sortable: true,
                        thClass: 'sortable',
                        formatFn: this.formatDate,
                    },
                    {
                        field: 'changedDate',
                        label: 'Changed',
                        sortable: true,
                        thClass: 'sortable',
                        formatFn: this.formatDate,
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
            formatDate(date) {
                return moment(date).format('M/D/YY hh:mm:ss A');  
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
                this.initializeGrid();
            },
            initializeGrid() {
                this.environments = [];

                axios.get("/api/FeatureToggles/environments", {
                    params: {
                        applicationId: this.selectedApp.id
                    }
                }).then((response) => {
                    this.environments = response.data;
                    this.createGridColumns();
                    this.loadGridData();
                    Bus.$emit(events.environmentsLoaded, this.environments)
                }).catch(error => Bus.$emit(events.showErrorAlertModal, { 'error': error }));
            },
            loadGridData() {
                this.getAllScheduledToggles();

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
                            createdDate: toggle.createdDate,
                            changedDate: toggle.changedDate,
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
                    Bus.$emit(events.togglesLoaded, gridRowModels);
                    Bus.$emit('unblock-ui')
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
                        environments: sch.environments,
                        scheduledDate: moment(sch.scheduledDate).format("YYYY-MM-DD"),
                        scheduledTime: new Date(sch.scheduledDate),
                        scheduledState: sch.scheduledState,
                        updatedBy: sch.updatedBy,
                        forceCacheRefresh:sch.forceCacheRefresh
                    };

                });
            },
            editToggleSchedule(schedule) {
                this.scheduleToEdit = schedule;
                this.showSchedulerModal = true;
            },
            openEditEnvironmentModal(column) {
                let environment = this.environments.find(element => element.envName == column.field);
                this.showEditEnvironmentModal = true;
                Bus.$emit(events.editEnvironment, environment);
            },
            openEditFeatureToggleModal(row) {
                this.showEditModal = true;
                Bus.$emit(events.openEditFeatureToggleModal, _.clone(row));
            },
            openDeleteFeatureToggleConfirmationModal(row) {
                this.showDeleteConfirmationModal = true
                Bus.$emit(events.deleteFeatureToggle, row);
            },
            refreshEnvironmentToggles(env, index) {
                if (!this.selectedApp)
                    return;

                Bus.$emit(events.blockUI)

                axios.post('api/CacheRefresh', {
                    applicationId: this.selectedApp.id,
                    envName: env
                })
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
                    }).catch(error => { Bus.$emit(events.showErrorAlertModal, { 'error': error })
                    }).finally(() => {
                        Bus.$emit(events.unblockUI)
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
