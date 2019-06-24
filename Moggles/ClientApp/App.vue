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
						<dropdown tag="li">
							<a class="dropdown-toggle" role="button">Tools <span class="caret"></span></a>
							<template slot="dropdown">
								<li><a role="button" @click="() => {this.ReloadCurrentApplicationToggles();}">Reload Application Toggles</a></li>
								<li><a role="button" @click="showAddToggle = true">Add Feature Toggle</a></li>
								<li><a role="button" @click="showAddApp = true">Add New Application</a></li>
								<li><a role="button" @click="showAddEnv = true">Add New Environment</a></li>
								<li v-if="isCacheRefreshEnabled"><a role="button" @click="showForceCacheRefresh = true">Force Cache Refresh</a></li>
							</template>
						</dropdown>
					</ul>
				</div><!-- /.navbar-collapse -->
			</div><!-- /.container-fluid -->
		</nav>

		<block-ui ref="blockUi"></block-ui>

		<modal v-model="showAddToggle" title="Add Feature Toggle" :footer="false">
			<add-featuretoggle></add-featuretoggle>
		</modal>

		<modal v-model="showAddApp" title="Add Application" :footer="false">
			<add-application></add-application>
		</modal>

		<modal v-model="showAddEnv" title="Add Environment" :footer="false">
			<add-env></add-env>
		</modal>

		<modal v-model="showForceCacheRefresh" title="Force Cache Refresh" :footer="false">
			<force-cache-refresh></force-cache-refresh>
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
	import TogglesList from "./TogglesList";
    import AppSelection from './AppSelection'
    import AddApplication from './AddApplication'
    import AddFeatureToggle from './AddFeatureToggle'
    import AddEnvironment from './AddEnvironment'
    import ForceCacheRefresh from './ForceCacheRefresh'
    import BlockUi from './BlockUi'
    import { Bus } from './event-bus'
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
			'block-ui': BlockUi
        }
    }
</script>