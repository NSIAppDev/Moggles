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
                    <a @click="edit(props.row)"><i class="fas fa-edit" /></a>
                    <a v-if="!props.row.isPermanent" @click="confirmDelete(props.row)"><i class="fas fa-trash-alt" /></a>
                    <span v-if="props.row.isPermanent" title="Permanent flags cannot be deleted!" class="disabled-link"><i class="fas fa-trash-alt" /></span>
                </span>
                <span v-else-if="props.column.field == 'toggleName' ">
                    <span>{{ props.row.toggleName }}</span> <span v-if="props.row.isPermanent" class="label label-danger">Permanent</span>
                    <a v-for="ft in getScheduled(props.row.toggleName)" :key="ft.id" @click="editToggleScheduler(ft)"><i class="fas fa-clock" /> <i /></a>
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
        <modal v-model="showDeleteConfirmation" title="You are about to delete a feature toggle" :footer="false">
            <delete-FeatureToggle/>
        </modal>
      
        <modal v-model="showScheduler" title="Schedule Toggles" :footer="false">
            <toggle-scheduler />
        </modal>
        <modal v-model="showEditModal" title="Edit Feature Flags" :footer="false">
            <edit-FeatureToggle />
        </modal>
        <modal v-model="showEditEnvironmentModal" title="Edit Environment" :footer="false">
            <edit-Environment />
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
    import EditFeatureToggle from './EditFeatureToggle';
    import EditEnvironment from './EditEnvironment';
    import DeleteFeatureToggle from './DeleteFeatureToggle';
    export default {
        components: {
            'p-check': PrettyCheck,
            'toggle-scheduler': ToggleScheduler,
            'edit-FeatureToggle': EditFeatureToggle,
            'edit-Environment': EditEnvironment,
            'delete-FeatureToggle': DeleteFeatureToggle
        },
        data() {

            return {
                toggles: [],
                gridColumns: [],
                selectedApp: {},
                rowToEdit: null,
                showEditEnvironmentModal: false,
                showEditModal: false,
                showDeleteConfirmation: false,
                environmentsToRefresh: [],
                refreshAlertVisible: false,
                isCacheRefreshEnabled: false,
                scheduledToggles: [],
                showScheduler: false,
                connectionId: null,
                connection: null,
                rowsPerPage: 10,
                environmentsList: [],
                environmentsNameList: []
            }
        },
        computed: {
            showRefreshAlert() {
                return this.environmentsToRefresh.length > 0 ? this.refreshAlertVisible : false;
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
                this.loadData();
                this.getAllScheduledToggle(this.selectedApp.id);
            })
            Bus.$on('close-editEnvironment', () => {
                this.showEditEnvironmentModal = false;
                this.loadData();
                
            })
            Bus.$on('close-editFeatureFlag', (environmentsToRefresh, refreshAlertVisible) => {
                this.showEditModal = false;
                this.loadData();
                this.environmentsToRefresh = environmentsToRefresh;
                this.refreshAlertVisible = refreshAlertVisible;
            })
            Bus.$on('close-deleteToggle', () => {
                this.showDeleteConfirmation = false;
                this.loadData();
            })
        },
        mounted() {
            this.start();
        },
        methods: {
            loadData() {
                this.initializeGrid(this.selectedApp);
                this.loadGridData(this.selectedApp.id);
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
            editEnvName(column) {
                var environmentFromList = this.environmentsList.find(element => element.envName == column.field)
                this.showEditEnvironmentModal = true
                Bus.$emit('edit-Environment', this.selectedApp, environmentFromList)
            },
            edit(row) {
                this.rowToEdit = _.clone(row)
                this.showEditModal = true;
                let toggle = {
                    appId : this.selectedApp.id,
                    rowToEdit : this.rowToEdit
                }
                Bus.$emit('open-editFeatureToggle', toggle, this.gridColumns);
            },
            confirmDelete(row) {
                this.showDeleteConfirmation = true
                let toggleToDelete = {
                    appId: this.selectedApp.id,
                    toggle: row
                }
                Bus.$emit('delete-featureToggle', toggleToDelete);
            },
            getAllScheduledToggle(appId) {

                axios.get("/api/toggleScheduler", {
                    params: {
                        applicationId: appId
                    }
                }).then((response) => {

                    //create the flattened row models
                    let models = _.map(response.data, toggle => {
                        return {
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
                    return {
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
            initializeGrid(app) {
                this.environmentsNameList = [];
                this.environmentsList = [];
                this.getAllScheduledToggle(app.id);
                axios.get("/api/FeatureToggles/environments", {
                    params: {
                        applicationId: app.id
                    }
                }).then((response) => {
                    this.environmentsList = response.data;
                    response.data.forEach(env => { if (!this.environmentsNameList.includes(env.envName)) { this.environmentsNameList.push(env.envName) } });
                    this.createGridColumns();
                    this.loadGridData(app.id);
                    this.$refs['toggleGrid'].reset()

                    Bus.$emit('env-loaded', this.environmentsNameList)
                }).catch((e) => { window.alert(e) });
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
            closeRefreshAlert() {
                this.refreshAlertVisible = false;
            },
            stringIsNullOrEmpty(text) {
                return !text || /^\s*$/.test(text);
            },
            isEnviroment(env) {
                return this.environmentsNameList.includes(env);
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
