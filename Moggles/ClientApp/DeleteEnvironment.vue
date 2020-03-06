<template>
  <div>
    <div>
      Are you sure you want to delete the environment?
      <br>
      All associated applications and feature toggles will be removed.
    </div>
    <div class="text-right">
      <button type="button" class="btn btn-default" @click="closeModal">
        Cancel
      </button>
      <button type="button" class="btn btn-primary" @click="deleteEnvironment">
        Delete
      </button>
    </div>
  </div>
</template>

<script>
    import axios from 'axios';
    import { Bus } from './event-bus';
    import _ from 'lodash';

    export default {
        data() {
            return {
                selectedApp: null,
                envName: ""
            }
        },
        created() {
            Bus.$on('delete-Environment', (environmentName, selectedApp) => {
                this.selectedApp = selectedApp;
                this.envName = environmentName;
            });
        },
        methods: {

            deleteEnvironment() {
                let environmentModel = {
                    applicationId: this.selectedApp.id,
                    envName: this.envName
                }

                axios.delete(`/api/FeatureToggles/environments`, { data: environmentModel }).then(() => {
                    this.showDeleteEnvironmentConfirmation = false;
                    this.showEditEnvironmentModal = false;
                    this.envName = null;
                    Bus.$emit('delete-EnvironmentRefresh', environmentModel.envName);
                    Bus.$emit("app-changed", this.selectedApp)
                    Bus.$emit('close-deleteEnvironment');
                    Bus.$emit('close-editEnvironment');

                }).catch(error => window.alert(error))
            },
            closeModal() {
                Bus.$emit('close-deleteEnvironment');
            },
            addEnvironemntToRefreshList(env) {
                let index = _.indexOf(this.environmentsToRefresh, env);
                if (index === -1 && this.isEnviroment(env)) {
                    this.environmentsToRefresh.push(env);
                    this.refreshAlertVisible = true;
                }
            },
        }
    }
</script>