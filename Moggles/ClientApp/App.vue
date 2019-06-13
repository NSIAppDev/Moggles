<template>
    <div>
        <nav class="navbar navbar-default navbar-fixed-top">
            <div class="container-fluid">
                <!-- Brand and toggle get grouped for better mobile display -->
                <div class="navbar-header">
                    <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#bs-example-navbar-collapse-1" aria-expanded="false">
                        <span class="sr-only">Toggle navigation</span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                        <span class="icon-bar"></span>
                    </button>
					<a class="navbar-brand" href="/">
						<img src="/img/Moggles-LogoType.png" alt="Moggles, Toggles for non development wizards" class="d-inline-block align-top" height="30" />
					</a>
                </div>

                <!-- Collect the nav links, forms, and other content for toggling -->
                <div class="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                    <ul class="nav navbar-nav navbar-left">
                        <li>
                            <div class="vertical-align">
                                <label for="app-sel">Select Application: &nbsp;</label>
                                <app-selection></app-selection>
                            </div>
                        </li>
                    </ul>
                    <ul class="nav navbar-nav navbar-right vertical-align">
                        <li class="dropdown">
                            <a href="#" class="dropdown-toggle" data-toggle="dropdown" role="button" aria-haspopup="true" aria-expanded="false">Tools <span class="caret"></span></a>
                            <ul class="dropdown-menu">
                                <li><a href="#" @click="() => {this.ReloadCurrentApplicationToggles();}">Reload Application Toggles</a></li>
                                <li><a href="#" @click="showAddToggle = true">Add Feature Toggle</a></li>
                                <li><a href="#" @click="showAddApp = true">Add New Application</a></li>
                                <li><a href="#" @click="showAddEnv = true">Add New Environment</a></li>
                                <li v-if="isCacheRefreshEnabled"><a href="#" @click="showForceCacheRefresh = true">Force Cache Refresh</a></li>
                            </ul>
                        </li>
                    </ul>
                </div><!-- /.navbar-collapse -->
            </div><!-- /.container-fluid -->
        </nav>

        <modal v-model="showAddToggle" title="Add Feature Toggle">
            <add-featuretoggle></add-featuretoggle>
            <div slot="modal-footer" class="modal-footer"></div>
        </modal>

        <modal v-model="showAddApp" title="Add Application">
            <add-application></add-application>
            <div slot="modal-footer" class="modal-footer"></div>
        </modal>

        <modal v-model="showAddEnv" title="Add Environment">
            <add-env></add-env>
            <div slot="modal-footer" class="modal-footer"></div>
        </modal>

        <modal v-model="showForceCacheRefresh" title="Force Cache Refresh">
            <force-cache-refresh></force-cache-refresh>
            <div slot="modal-footer" class="modal-footer"></div>
        </modal>

        <div class="container-fluid">
            <div class="row">
                <div class="col-md-12">
                    <toggles-list></toggles-list>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    import Vue from 'vue'
	import TogglesList from "./TogglesList";
    import AppSelection from './AppSelection'
    import AddApplication from './AddApplication'
    import AddFeatureToggle from './AddFeatureToggle'
    import AddEnvironment from './AddEnvironment'
    import ForceCacheRefresh from './ForceCacheRefresh'
    import { Bus } from './event-bus'
    import { modal } from 'vue-strap'
    import axios from 'axios'
	
    export default {
        data() {
            return {
                showAddApp: false,
                showAddEnv: false,
                showAddToggle: false,
                showForceCacheRefresh: false,
                isCacheRefreshEnabled: false
            }
        },
        methods: {
            ReloadCurrentApplicationToggles(){
                Bus.$emit("reload-application-toggles");
            }
        },
        created() {
            axios.get("/api/CacheRefresh/getCacheRefreshAvailability").then((response) => {
                this.isCacheRefreshEnabled = response.data;
            }).catch(error => { window.alert(error) });
        },
        components: {
			"toggles-list": TogglesList,
            "app-selection": AppSelection,
            "add-application": AddApplication,
            "add-featuretoggle": AddFeatureToggle,
            "add-env": AddEnvironment,
            'force-cache-refresh': ForceCacheRefresh,
            modal
        }
    }
</script>
<style lang="scss">
    $main-bg-color: #ffffff;

    body {
        padding-top: 70px;
        background: $main-bg-color;
    }

    .margin-right-10 {
        margin-right: 10px;
    }

    .vertical-align {
        display: flex;
        align-items: center;
        height: 50px;
    }
</style>
