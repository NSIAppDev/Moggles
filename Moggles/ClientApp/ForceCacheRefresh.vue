<template>
    <div>
        <spinner ref="spinner" v-model="spinner" size="sm"></spinner>

        <alert v-model="showSuccessAlert" placement="top-right" duration="1500" type="success" width="400px" dismissable>
            <span class="icon-ok-circled alert-icon-float-left"></span>
            <p>Cache Refreshed.</p>
        </alert>

        <div class="panel-body">
            <div class="">
                <div class="form-group">
                    <label>Select environment for which to refresh the cache:</label>
                    <select class="form-control" v-model="envName" required id="environmentSelect">
                        <option v-for="env in existingEnvs">{{ env }}</option>
                    </select>
                </div>
                <button id="refreshBtn" :disabled="applicationId > 0 && envName ? false : true" class="btn btn-default btn-primary" v-on:click="refresh" type="button">Refresh</button>
            </div>
        </div>
    </div>
</template>

<script>
    import { Bus } from './event-bus';
    import axios from 'axios';
    import { spinner, alert } from 'vue-strap';

    export default {
        data() {
            return {
                applicationId: -1,
                existingEnvs: [],
                spinner: false,
                showSuccessAlert: false,
                envName: null
            };
        },
        methods: {
            refresh() {
                if (this.applicationId === -1)
                    return;

                let param = {
                    applicationId: this.applicationId,
                    envName: this.envName
                };

                this.spinner = true;
                axios.post('api/CacheRefresh', param)
                    .then((response) => {
                        this.spinner = false;
                        this.showSuccessAlert = true;
                        this.envName = null;
                    }).catch((e) => {
                        window.alert(e);
                    }).finally(() => {
                        this.spinner = false;
                    });
            }
        },
        mounted() {
            Bus.$on("app-changed", app => {
                if (app) {
                    this.applicationId = app.id;
                }
            });

            Bus.$on("env-loaded", envs => {
                this.existingEnvs = envs;
            });
        },
        components: {
            spinner,
            alert
        }
    }
</script>
