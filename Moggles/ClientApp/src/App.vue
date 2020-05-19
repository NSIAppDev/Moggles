<template>
  <div>
    <nav class="navbar navbar-default navbar-fixed-top">
      <div class="container-fluid">
        <!-- Brand and toggle get grouped for better mobile display -->
        <div class="navbar-header">
          <button type="button" class="navbar-toggle collapsed" data-toggle="collapse"
                  data-target="#bs-example-navbar-collapse-1" aria-expanded="false">
            <span class="sr-only">Toggle navigation</span>
            <span class="icon-bar" />
            <span class="icon-bar" />
            <span class="icon-bar" />
          </button>
          <a class="navbar-brand" href="/">
            <img src="/img/Moggles-LogoType.png" alt="Moggles, Toggles for non development wizards" class="d-inline-block align-top"
                 height="30">
          </a>
        </div>

        <!-- Collect the nav links, forms, and other content for toggling -->
        <div id="bs-example-navbar-collapse-1" class="collapse navbar-collapse">
          <ul class="nav navbar-nav navbar-left">
            <li>
              <div class="vertical-align">
                <label for="app-sel" class="margin-top-8">Select Application </label>
                <app-selection />
                <a class="margin-left-10" @click="showEditAppModal(true)"><i id="showEditApplicationModalBtn" class="fas fa-edit fa-lg" /></a>
              </div>
            </li>
          </ul>
          <ul class="nav navbar-nav navbar-right vertical-align">
            <dropdown tag="li">
              <a id="toolsBtn" class="dropdown-toggle" role="button">Tools <span class="caret" /></a>
              <template slot="dropdown">
                <li><a role="button" @click="reloadCurrentApplicationToggles()">Reload Application Toggles</a></li>
                <li><a role="button" @click="showAddFeatureToggleModal()">Add Feature Toggle</a></li>
                <li><a role="button" @click="showAddAppModal()">Add New Application</a></li>
                <li><a role="button" @click="showAddEnvModal()">Add New Environment</a></li>
                <li><a role="button" @click="showAddFeatureToggleScheduleModal()">Add New Feature Toggle Schedule</a></li>
                <li v-if="isCacheRefreshEnabled">
                  <a role="button" @click="showForceCacheRefresh = true">Force Cache Refresh</a>
                </li>
              </template>
            </dropdown>
          </ul>
        </div><!-- /.navbar-collapse -->
      </div><!-- /.container-fluid -->
    </nav>

    <block-ui ref="blockUi" />

    <modal v-if="showAddToggle" v-model="showAddToggle" title="Add Feature Toggle"
           :footer="false">
      <add-featuretoggle :application="selectedApp" />
    </modal>

    <modal v-model="showAddApp" title="Add Application" :footer="false">
      <add-application />
    </modal>

    <modal v-model="showAddEnv" title="Add Environment" :footer="false">
      <add-env :application="selectedApp" />
    </modal>

    <modal v-model="showForceCacheRefresh" title="Force Cache Refresh" :footer="false">
      <force-cache-refresh />
    </modal>

    <modal v-if="editAppModalIsActive" v-model="editAppModalIsActive" title="Edit Application"
           :footer="false">
      <edit-application :application="selectedApp" @close-app-edit-modal="showEditAppModal(false)" />
    </modal>

    <modal v-if="showDeleteAppConfirmation" v-model="showDeleteAppConfirmation" title="You are about to delete an application"
           :footer="false">
      <delete-application :application="selectedApp" @cancel="showDeleteAppConfirmation = false" @deleteAppCompleted="showEditAppModal(false)" />
    </modal>

    <modal v-if="showScheduler" v-model="showScheduler" title="Schedule Toggles"
           :footer="false">
      <add-toggle-schedule :application="selectedApp" :is-cache-refresh-enabled="isCacheRefreshEnabled" />
    </modal>

    <div class="container-fluid">
      <div class="row">
        <div class="col-md-12">
          <toggles-list />
        </div>
      </div>
    </div>
  </div>
</template>
<script>
    import TogglesList from "./TogglesList";
    import AppSelection from './menu/AppSelection'
    import AddApplication from './application/AddApplication'
    import EditApplication from './application/EditApplication'
    import DeleteApplication from './application/DeleteApplication'
    import AddFeatureToggle from './featureToggle/AddFeatureToggle'
    import AddEnvironment from './environment/AddEnvironment'
    import ForceCacheRefresh from './menu/ForceCacheRefresh'
    import AddToggleSchedule from './featureToggleSchedule/AddToggleSchedule'
    import BlockUi from './common/BlockUi'
    import { Bus } from './common/event-bus'
    import axios from 'axios'
    import { events } from './common/events';

    export default {
        components: {
            "toggles-list": TogglesList,
            "app-selection": AppSelection,
            "add-application": AddApplication,
            "edit-application": EditApplication,
            "delete-application": DeleteApplication,
            "add-featuretoggle": AddFeatureToggle,
            "add-env": AddEnvironment,
            'force-cache-refresh': ForceCacheRefresh,
            'block-ui': BlockUi,
            'add-toggle-schedule': AddToggleSchedule
        },
        data() {
            return {
                showAddApp: false,
                showAddEnv: false,
                showAddToggle: false,
                showForceCacheRefresh: false,
                isCacheRefreshEnabled: false,
                editAppModalIsActive: false,
                showDeleteAppConfirmation: false,
                showScheduler: false,
                selectedApp: {}
            }
        },
        created() {
            Bus.$on(events.applicationChanged, app => {
                if (app) {
                    this.selectedApp = app;
                }
            });

            Bus.$on(events.showDeleteApplicationConfirmationModal, () => {
                this.showDeleteAppConfirmation = true;
            });

            Bus.$on(events.closeAddFeatureToggleModal, () => {
                this.showAddToggle = false;
            });

            Bus.$on(events.closeAddApplicationModal, () => {
                this.showAddApp = false;
            });

            Bus.$on(events.closeAddEnvironmentModal, () => {
                this.showAddEnv = false;
            });

            Bus.$on(events.closeForceCacheRefreshModal, () => {
                this.showForceCacheRefresh = false;
            });

            Bus.$on(events.closeToggleSchedulerModal, () => {
                this.showScheduler = false;
            });

            axios.get("/api/CacheRefresh/getCacheRefreshAvailability").then((response) => {
                this.isCacheRefreshEnabled = response.data;
            }).catch(error => { window.alert(error) });
        },
        methods: {
            showAddFeatureToggleModal() {
                this.showAddToggle = true;
                Bus.$emit(events.openAddFeatureToggleModal);
            },
            showAddAppModal() {
                this.showAddApp = true;
                Bus.$emit(events.openAddApplicationModal);
            },
            showAddEnvModal() {
                this.showAddEnv = true;
                Bus.$emit(events.openAddEnvironmentModal);
            },
            reloadCurrentApplicationToggles() {
                Bus.$emit(events.reloadApplicationToggles);
            },
            showEditAppModal(value) {
                this.editAppModalIsActive = value;
            },
            confirmDeleteApp() {
                this.showDeleteAppConfirmation = true;
            },
            showAddFeatureToggleScheduleModal() {
                this.showScheduler = true;
                Bus.$emit(events.openAddSchedulerModal);
            }
        }
    }
</script>