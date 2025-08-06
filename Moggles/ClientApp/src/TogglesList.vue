<template>
  <div>
    <alert-cacheRefresh :show-refresh-alert="showRefreshAlert" :environments-to-refresh="environmentsToRefresh" :selected-app="selectedApp" />
    <vue-good-table id="toggleGrid" ref="toggleGrid"
                    :columns="gridColumns"
                    :rows="toggles"
                    :pagination-options="getPaginationOptions"
                    :sort-options="{
                      enabled: true,
                      initialSortBy: {field: 'isPermanent', type: 'desc'}
                    }"
                    style-class="vgt-table striped condensed bordered"
                    :class="{ 'disabled-table': disableGrid }"
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
        <span v-else-if="props.column.field == 'status'">
          <span>{{ getStatusValue(props.row) }}</span>
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

    <modal v-model="showDeleteConfirmationModal" title="You are about to delete a feature toggle" :footer="false"
           append-to-body>
      <delete-featureToggle :application="selectedApp" />
    </modal>
    <modal v-if="showSchedulerModal" v-model="showSchedulerModal" title="Edit Feature Toggle Schedule"
           :footer="false" append-to-body>
      <edit-toggle-schedule :application="selectedApp" :is-cache-refresh-enabled="isCacheRefreshEnabled" :schedule="scheduleToEdit" />
    </modal>
    <modal v-model="showEditModal" title="Edit Feature Flag" :footer="false"
           append-to-body>
      <edit-featureToggle :application="selectedApp" :is-cache-refresh-enabled="isCacheRefreshEnabled" />
    </modal>
    <modal v-model="showEditEnvironmentModal" title="Edit Environment" :footer="false"
           append-to-body>
      <edit-environment :application="selectedApp" :environment-count="environments.length" />
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
  import AlertCacheRefresh from './common/AlertCacheRefresh';

  export default {
    components: {
      'p-check': PrettyCheck,
      'edit-toggle-schedule': EditToggleSchedule,
      'edit-featureToggle': EditFeatureToggle,
      'edit-environment': EditEnvironment,
      'delete-featureToggle': DeleteFeatureToggle,
      'alert-cacheRefresh': AlertCacheRefresh
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
        isCacheRefreshEnabled: false,
        statusValues: ["Unaccepted", "Accepted", "On Hold"],
      }
    },
    computed: {
      showRefreshAlert() {
        return this.environmentsToRefresh.length > 0 && this.isCacheRefreshEnabled ? this.isRefreshAlertVisible : false;
      },
      getPaginationOptions() {
        return { enabled: true, perPage: parseInt(this.getRowsPerPage()), dropdownAllowAll: false };
      },
      environmentsNameList() {
        return _.map(this.environments, (env) => {
          return env.envName;
        });
      },
      hasBeenMigrated() {
        return this.selectedApp.hasBeenMigrated;
      },
      disableGrid() {
        return this.selectedApp.isDeleted;
      }
    },
    watch: {
      hasBeenMigrated() {
        this.createGridColumns();
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
          this.selectedApp.assignedTo = applicationUpdateModel.applicationAssignedTo
        })

        Bus.$on(events.environmentAdded, () => {
          this.loadGrid();
        })

        Bus.$on(events.environmentMoved, () => {
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
            width: '45px'
          },
          {
            field: 'toggleName',
            label: 'Feature Toggle Name',
            sortable: true,
            thClass: 'sortable',
            width: '200px',
            filterOptions: {
              enabled: true,
              placeholder: 'Filter Toggle Name'
            }
          },
          {
            field: 'workItemIdentifier',
            label: 'Work Item ID',
            sortable: true,
            width: '150px',
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
            width: '300px',
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
            field: 'status',
            label: 'Status',
            type: 'int',
            sortable: false,
            hidden: !this.hasBeenMigrated,
            width: '140px',
            filterOptions: {
              enabled: this.hasBeenMigrated,
              filterDropdownItems: [
                { value: '1', text: 'Accepted' },
                { value: '0', text: 'Unaccepted' },
                { value: '2', text: 'On Hold' }
              ],
              filterValue: '0',
              placeholder: 'All'
            }
          },
          {
            field: 'userAccepted',
            label: 'User Accepted',
            type: 'boolean',
            sortable: false,
            width: '140px',
            hidden: this.hasBeenMigrated,
            filterOptions: {
              enabled: !this.hasBeenMigrated,
              filterDropdownItems: [
                { value: 'true', text: 'Accepted' },
                { value: 'false', text: 'Unaccepted' }
              ],
              filterValue: 'false',
              placeholder: 'All'
            }
          },
          {
            field: 'progressStatus',
            label: 'Progress Status',
            type: 'string',
            sortable: false,
            width: '140px',
            filterOptions: {
              enabled: true,
              filterDropdownItems: [
                { value: 'Development', text: 'Development' },
                { value: 'Testing', text: 'Testing' },
                { value: 'BPO Ready', text: 'BPO Ready' },
                { value: 'BPO Notified', text: 'BPO Notified' },
                { value: 'Live Validation', text: 'Live Validation' }
              ],
              placeholder: 'All'
            }
          },
          {
            field: 'createdDate',
            label: 'Created',
            sortable: true,
            width: '140px',
            thClass: 'sortable',
            formatFn: this.formatDate,
          },
          {
            field: 'changedDate',
            label: 'Changed',
            sortable: true,
            width: '140px',
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
            width: '100px',
            filterOptions: {
              enabled: true,
              filterDropdownItems: [
                { value: 'true', text: 'On' },
                { value: 'false', text: 'Off' }
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
              status: toggle.status,
              progressStatus: toggle.progressStatus,
              holdReason: toggle.holdReason,
              workItemIdentifier: toggle.workItemIdentifier,
              createdDate: toggle.createdDate,
              changedDate: toggle.changedDate,
              reasonsToChange: toggle.reasonsToChange
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
            forceCacheRefresh: sch.forceCacheRefresh
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
        Bus.$emit(events.blockUI);
        this.showEditModal = true;
        row.showStatus = this.hasBeenMigrated;
        Bus.$emit(events.openEditFeatureToggleModal, _.clone(row));
      },
      openDeleteFeatureToggleConfirmationModal(row) {
        this.showDeleteConfirmationModal = true
        Bus.$emit(events.deleteFeatureToggle, row);
      },
      isEnvironmentColumn(column) {
        return (column.type == 'boolean' && column.field != 'userAccepted' && column.field != 'status');
      },
      getStatusValue(row) {
        return this.statusValues[row.status];
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

  .disabled-table {
    pointer-events: none;
    opacity: 0.5;
    user-select: none;
  }
</style>
