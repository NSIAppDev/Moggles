<template>
  <div class="container-fluid">
    <div class="row">
      <div class="col-md-8">
        <h1>Global search</h1>
      </div>
    </div>
    <div class="form-horizontal panel panel-default">
      <div class="row" style="padding: 25px">
        <div class="col-md-6">
          <div class="form-group">
            <label class="control-label col-md-4" for="keyword">Keyword</label>
            <div class="col-md-8">
              <input id="keyword" v-model="globalSearch.keyword" type="text" 
                     class="form-control" placeholder="Enter keyword...">
            </div>
          </div>
          <div class="form-group">
            <label class="control-label col-md-4" for="application">Application</label>
            <multi-select id="selectedApps" v-model="globalSearch.applicationIds" class="col-md-8"
                          filterable placeholder="Select application" :options="applications" 
                          :value-key="'id'" 
                          :label-key="'appName'"
                          :selected-icon="'fas fa-check'" append-to-body />
          </div>
          <div class="form-group">
            <label class="control-label col-md-4" for="status">Status</label>
            <multi-select v-model="statusAsArray" class="col-md-8" :options="progressStatusOptionsMapped"
                          :value-key="'id'" :label-key="'label'" placeholder="Select status" 
                          :selected-icon="'fas fa-check'"
                          :limit="1" append-to-body />
          </div>
          <div class="form-group">
            <label class="control-label col-md-4">Created Within:</label>
            <div class="col-md-4">
              <input v-model="globalSearch.createdStart" type="date" class="form-control">
            </div>
            <div class="col-md-4">
              <input v-model="globalSearch.createdEnd" type="date" class="form-control">
            </div>
          </div>
          <div v-for="(env, index) in globalSearch.environments" :key="index" class="form-group">
            <label class="col-md-4 control-label">Environment:</label>
            <div class="col-md-4">
              <input v-model="env.name" class="form-control" type="text"
                     @input="onEnvironmentNameChange(env)">
            </div>
            <div class="col-md-2">
              <input v-model="env.enabled" type="radio" :value="true"
                     :disabled="!env.name"> On
            </div>
            <div class="col-md-2">
              <input v-model="env.enabled" type="radio" :value="false"
                     :disabled="!env.name"> Off
            </div>
          </div>
        </div>
        <div class="col-md-6">
          <div class="form-group">
            <label class="col-md-4 control-label" for="workItemIdentifier">Work Item ID:</label>
            <div class="col-md-8">
              <input id="workItemIdentifier" v-model="globalSearch.workItemIdentifier" type="text"
                     class="form-control">
            </div>
          </div>
          <div class="form-group">
            <label class="col-md-4 control-label" for="assignedTo">Assigned To:</label>
            <multi-select v-model="globalSearch.assignedTo" class="col-md-8" :options="assignedToOptions"
                          :value-key="'id'" 
                          :label-key="'label'" placeholder="Select Assigned To" 
                          :selected-icon="'fas fa-check'"
                          append-to-body />
          </div>
          <div class="form-group">
            <label class="col-md-4 control-label">Permanent:</label>
            <div class="col-md-8">
              <div class="col-md-3">
                <input v-model="globalSearch.isPermanent" type="radio" :value="null"> All
              </div>
              <div class="col-md-4">
                <input v-model="globalSearch.isPermanent" type="radio" :value="true"> Is Permanent
              </div>
              <div class="col-md-5">
                <input v-model="globalSearch.isPermanent" type="radio" :value="false"> Is Not Permanent
              </div>
            </div>
          </div>
          <div class="form-group">
            <label class="col-md-4 control-label">Changed Within:</label>
            <div class="col-md-4">
              <input v-model="globalSearch.changedStart" type="date" class="form-control">
            </div>
            <div class="col-md-4">
              <input v-model="globalSearch.changedEnd" type="date" class="form-control">
            </div>
          </div>
          <div class="form-group">
            <label class="col-md-4 control-label">User Accepted:</label>
            <div class="col-md-8">
              <div class="col-md-3">
                <input v-model="globalSearch.userAccepted" type="radio" :value="null"> All
              </div>
              <div class="col-md-4">
                <input v-model="globalSearch.userAccepted" type="radio" :value="true"> Accepted
              </div>
              <div class="col-md-5">
                <input v-model="globalSearch.userAccepted" type="radio" :value="false"> Unaccepted
              </div>
            </div>
          </div>
          <div class="form-group">
            <div class="col-md-12">
              <div class="pull-right">
                <a class="btn btn-default" @click="$emit('back')">
                  Back
                </a>
                <a class="btn btn-default" @click="reset">
                  Reset
                </a>
                <button class="btn btn-primary" @click="search">
                  Search
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div v-if="featureToggles && featureToggles.length > 0">
      <vue-good-table id="toggleGrid" ref="toggleGrid" :columns="gridColumns" 
                      :pagination-options="paginationOptions"
                      :rows="featureToggles" style-class="vgt-table striped condensed bordered">
        <template slot="table-row" slot-scope="props">
          <span v-if="props.column.field === 'environments'" class="env-cell">
            <span v-for="(env, index) in props.formattedRow.environments" :key="index" class="env-item">
              <p-check v-model="env.enabled" :disabled="!env.isDeployed" :color="env.isDeployed ? 'success' : 'default'"
                       class="p-icon p-fill p-locked">
                <i slot="extra" class="icon fas fa-check" />
              </p-check>
              <span class="env-label">{{ env.environment }}</span>
            </span>
          </span>

          <span v-else-if="props.column.type == 'boolean'" class="pull-left">
            <p-check v-model="props.formattedRow[props.column.field]" class="p-icon p-fill p-locked" color="default">
              <i slot="extra" class="icon fas fa-check" />
            </p-check>
          </span>

          <span v-else-if="props.column.field == 'id'">
            <a @click="openEditFeatureToggleModal(props.row)"><i class="fas fa-edit" /></a>
            <a v-if="!props.row.isPermanent" @click="openDeleteFeatureToggleConfirmationModal(props.row)"><i
              class="fas fa-trash-alt" /></a>
            <span v-if="props.row.isPermanent" title="Permanent flags cannot be deleted!" class="disabled-link"><i
              class="fas fa-trash-alt" /></span>
          </span>

          <span v-else>
            {{ props.formattedRow[props.column.field] }}
          </span>
        </template>
      </vue-good-table>
      <modal v-model="showEditModal" title="Edit Feature Flag" :footer="false" 
             append-to-body>
        <edit-featureToggle :application="selectedApp" :is-cache-refresh-enabled="isCacheRefreshEnabled" />
      </modal>
      <modal v-model="showDeleteConfirmationModal" title="You are about to delete a feature toggle" :footer="false"
             append-to-body>
        <delete-featureToggle :application="selectedApp" />
      </modal>
    </div>
    <div v-if="featureToggles && featureToggles.length === 0" slot="emptystate">
      <div class="text-center">
        There are no toggles for this application or filtered search
      </div>
    </div>
  </div>
</template>

<script>
import PrettyCheck from 'pretty-checkbox-vue/check';
import { Bus } from '../common/event-bus'
import { events } from '../common/events';
import moment from 'moment';
import axios from 'axios';
import _ from 'lodash';
import GlobalSearchModel from './models/GlobalSearchModel';
import EditFeatureToggle from './../featureToggle/EditFeatureToggle';
import DeleteFeatureToggle from './../featureToggle/DeleteFeatureToggle';

export default {
  components: {
    'p-check': PrettyCheck,
    'edit-featureToggle': EditFeatureToggle,
    'delete-featureToggle': DeleteFeatureToggle
  },
  data() {
    return {
      globalSearch: GlobalSearchModel(),
      applications: [],
      progressStatusOptionsMapped: [],
      progressStatusOptionsRawData: [],
      statusAsArray: [],
      assignedToOptions: [],
      featureToggles: null,
      gridColumns: [],
      paginationOptions: {
        enabled: true,
        perPage: 10,
        perPageDropdown: [10, 20, 50],
        dropdownAllowAll: false
      },
      showEditModal: false,
      selectedApp: {},
      isCacheRefreshEnabled: false,
      showDeleteConfirmationModal: false,
    }
  },
  watch: {
    statusAsArray(val) {
      this.globalSearch.progressStatus = val.length ? val[0] : '';
    }
  },
  mounted() {
  },
  created() {
    this.getCacheRefreshAvailability();
    this.subscribeToBusEvents();
    this.getApplicationOptions();
    this.fetchProgressStatusOptions();
    this.getAssignedToOptions();
  },
  methods: {
    subscribeToBusEvents() {
      Bus.$on(events.closeEditFeatureToggleModal, () => {
        this.showEditModal = false;
        this.search();
      })
      Bus.$on(events.closeDeleteFeatureToggleModal, () => {
        this.showDeleteConfirmationModal = false;
        this.search();
      })
    },
    getCacheRefreshAvailability() {
      axios.get("/api/CacheRefresh/getCacheRefreshAvailability").then((response) => {
        this.isCacheRefreshEnabled = response.data;
      }).catch(error => Bus.$emit(events.showErrorAlertModal, { 'error': error }));
    },
    onEnvironmentNameChange(env) {
      if (env.name && env.enabled === null) {
        this.$set(env, 'enabled', true);
      } else if (!env.name) {
        this.$set(env, 'enabled', null);
      }
    },
    getApplicationOptions() {
      axios.get('/api/applications')
        .then((response) => {
          this.applications = response.data;
        }).catch(error => {
          Bus.$emit(events.showErrorAlertModal, { 'error': error });
        });
    },
    fetchProgressStatusOptions() {
      axios.get('/api/FeatureToggles/status-options')
        .then(response => {
          const rawData = response.data || [];
          this.progressStatusOptionsRawData = response.data;
          this.progressStatusOptionsMapped = rawData.map(item => ({
            id: item,
            label: item
          }));
        })
        .catch(() => {
          this.progressStatusOptionsMapped = [];
          this.progressStatusOptionsRawData = [];
        });
    },
    getAssignedToOptions() {
      axios.get('/api/applications/assignedto-options')
        .then(response => {
          const rawData = response.data || [];
          this.assignedToOptions = rawData.map(item => ({
            id: item,
            label: item
          }));
        });
    },
    search() {
      axios.post('api/globalSearch', this.globalSearch)
        .then(response => {
          this.featureToggles = response.data;
          this.$nextTick(() => {
            this.createGridColumns();
          });
        });
    },
    reset() {
      this.globalSearch = new GlobalSearchModel();
      this.statusAsArray = [];
      this.featureToggles = null;
    },
    createGridColumns() {
      if (this.$refs['toggleGrid'] && typeof this.$refs['toggleGrid'].reset === 'function') {
        this.$refs['toggleGrid'].reset();
      }
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
          field: 'applicationName',
          label: 'Application',
          width: '150px',
          filterOptions: {
            enabled: true,
            placeholder: 'Application'
          }
        },
        {
          label: 'Environments',
          field: 'environments',
          width: '200px'
        },
        {
          field: 'assignedTo',
          label: 'Assigned To',
          width: '150px',
          filterOptions: {
            enabled: true,
            placeholder: 'Assigned To'
          }
        },
        {
          field: 'workItemIdentifier',
          label: 'Work Item ID',
          width: '150px',
          filterOptions: {
            enabled: true,
            placeholder: 'Filter Work Item ID'
          }
        },
        {
          field: 'notes',
          label: 'Notes',
          width: '300px',
          filterOptions: {
            enabled: true,
            placeholder: 'Filter Notes'
          }
        },
        {
          field: 'progressStatus',
          label: 'Status',
          type: 'string',
          sortable: false,
          width: '140px',
          filterOptions: {
            enabled: true,
            filterDropdownItems: this.progressStatusOptionsRawData,
            placeholder: 'All'
          }
        },
        {
          field: 'userAccepted',
          label: 'User Accepted',
          type: 'boolean',
          sortable: false,
          width: '140px',
          filterOptions: {
            enabled: true,
            filterDropdownItems: [
              { value: 'true', text: 'Accepted' },
              { value: 'false', text: 'Unaccepted' }
            ],
            placeholder: 'All'
          }
        },
        {
          field: 'createdDate',
          label: 'Created',
          width: '140px',
          formatFn: this.formatDate,
        },
        {
          field: 'changedDate',
          label: 'Changed',
          width: '140px',
          formatFn: this.formatDate,
        },
      ];

      this.gridColumns = columns;
    },
    formatDate(date) {
      return moment(date).format('M/D/YY hh:mm:ss A');
    },
    openEditFeatureToggleModal(row) {
      this.selectedApp = this.applications.find(a => a.id == row.applicationId);
      Bus.$emit(events.blockUI);
      this.$nextTick(() => {
        this.showEditModal = true;
        Bus.$emit(events.openEditFeatureToggleModal, _.clone(row));
      });
    },
    openDeleteFeatureToggleConfirmationModal(row) {
      this.selectedApp = this.applications.find(a => a.id == row.applicationId);
      this.$nextTick(() => {
        this.showDeleteConfirmationModal = true
        Bus.$emit(events.deleteFeatureToggle, row);
      });
    },
  }
}
</script>
<style scoped>
.env-cell {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.env-item {
  display: flex;
  align-items: center;
  gap: 4px;
}
</style>
