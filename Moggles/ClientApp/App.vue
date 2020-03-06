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
                <a class="margin-left-10" @click="showEditAppModal(true)"><i class="fas fa-edit fa-lg" /></a>
              </div>
            </li>
          </ul>
          <ul class="nav navbar-nav navbar-right vertical-align">
            <dropdown tag="li">
              <a class="dropdown-toggle" role="button">Tools <span class="caret" /></a>
              <template slot="dropdown">
                <li><a role="button" @click="() => {ReloadCurrentApplicationToggles();}">Reload Application Toggles</a></li>
                <li><a role="button" @click="showAddFeatureModal()">Add Feature Toggle</a></li>
                <li><a role="button" @click="showAddAppModal()">Add New Application</a></li>
                <li><a role="button" @click="showAddEnvModal()">Add New Environment</a></li>
                <li><a role="button" @click="showAddScheduler()">Feature Toggle Scheduler</a></li>
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

    <modal v-model="showAddToggle" title="Add Feature Toggle" :footer="false">
      <add-featuretoggle />
    </modal>

    <modal v-model="showAddApp" title="Add Application" :footer="false">
      <add-application />
    </modal>

    <modal v-model="showAddEnv" title="Add Environment" :footer="false">
      <add-env />
    </modal>

    <modal v-model="showForceCacheRefresh" title="Force Cache Refresh" :footer="false">
      <force-cache-refresh />
    </modal>

    <modal v-model="editAppModalIsActive" title="Edit Application" :footer="false">
      <edit-application @close-app-edit-modal="showEditAppModal(false)" />
    </modal>

    <modal v-model="showDeleteAppConfirmation" title="You are about to delete an application" :footer="false">
      <delete-application @cancel="showDeleteAppConfirmation = false" @deleteAppCompleted="showEditAppModal(false)" />
    </modal>

    <modal v-model="showScheduler" title="Schedule Toggles" :footer="false">
      <toggle-scheduler :is-cache-refresh-enabled="isCacheRefreshEnabled" />
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
    import AppSelection from './AppSelection'
    import AddApplication from './AddApplication'
    import EditApplication from './EditApplication'
    import DeleteApplication from './DeleteApplication'
    import AddFeatureToggle from './AddFeatureToggle'
    import AddEnvironment from './AddEnvironment'
    import ForceCacheRefresh from './ForceCacheRefresh'
    import ToggleScheduler from './ToggleScheduler'
    import BlockUi from './BlockUi'
    import { Bus } from './event-bus'
    import axios from 'axios'
	
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
            'toggle-scheduler': ToggleScheduler
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
                showScheduler: false
            }
        },
        created() {
            Bus.$on("show-app-delete-confirmation", () => {
                this.showDeleteAppConfirmation = true;
			})
			Bus.$on('close-add-toggle', () => {
				this.showAddToggle = false;
			})
			Bus.$on('close-add-application', () => {
				this.showAddApp = false;
			})
			Bus.$on('close-add-environment', () => {
				this.showAddEnv = false;
			})
			Bus.$on('close-refresh', () => {
				this.showForceCacheRefresh = false;
            })
            Bus.$on('close-scheduler', () => {
                this.showScheduler = false;
			})
            axios.get("/api/CacheRefresh/getCacheRefreshAvailability").then((response) => {
                this.isCacheRefreshEnabled = response.data;
            }).catch(error => { window.alert(error) });
        },
        methods: {
            showAddFeatureModal() {
                this.showAddToggle = true;
                Bus.$emit('openAddFeatureToggleModal');
            },
            showAddAppModal() {
                this.showAddApp = true;
                Bus.$emit('openAddAppModal');
            },
            showAddEnvModal() {
                this.showAddEnv = true;
                Bus.$emit('openAddEnvModal');
            },
            ReloadCurrentApplicationToggles(){
                Bus.$emit("reload-application-toggles");
            },
            showEditAppModal(value) {
                this.editAppModalIsActive = value;
            },
            confirmDeleteApp() {
                this.showDeleteAppConfirmation = true;
            },
            showAddScheduler() {
                this.showScheduler = true;
                Bus.$emit('add-scheduler');
            }
        }
    }
</script>