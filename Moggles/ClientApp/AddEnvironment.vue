<template>
    <div>
        <spinner ref="spinner" v-model="spinner" size="sm"></spinner>

        <alert v-model="showSuccessAlert" placement="top-right" duration="1500" type="success" width="400px" dismissable>
            <span class="icon-ok-circled alert-icon-float-left"></span>
            <p>Environment added.</p>
        </alert>

        <div class="panel-body">
            <div class="">
                <div class="form-group">
                    <div v-for="error in errors" :key="error" class="validationMessage">{{error}}</div>
                    <input class="form-control" v-model="envName" type="text" name="envName" placeholder="Env name..." maxlength="50">
                </div>
                <div class="form-group">
                    <div>
                        Default toggle value:
                    </div>
                    <div>
                        <label for="d1">True</label>
                        <input id="d1" v-model="defaultToggleValue" type="radio" name="defToggleValue" value="true" checked>

                        <label for="d2">False</label>
                        <input id="d2" v-model="defaultToggleValue" type="radio" name="defToggleValue" value="false">
                    </div>
                </div>
                <button :disabled="applicationId > 0? false : true" class="btn btn-default btn-primary" v-on:click="addEnv" type="button">Add</button>
            </div>
        </div>
    </div>
</template>

<script>
    import { Bus } from './event-bus'
    import axios from 'axios'
    import { spinner, alert } from 'vue-strap'

    export default {
        data() {
            return {
                applicationId: -1,
                envName: "",
                sortOrder: 500,
                defaultToggleValue: true,
                existingEnvs: [],
                errors: [],
                spinner: false,
                showSuccessAlert: false
            }
        },
        methods: {
            addEnv() {
                if (this.applicationId === -1)
                    return;

                this.errors = [];

                if (this.existingEnvs.some(env => env === this.envName)) {
                    this.errors.push("Environment already exists")
                    return;
                }

                if (this.envName === "") {
                    this.errors.push("Environment name cannot be empty")
                    return;
                }

                let param = {
                    applicationId: this.applicationId,
                    envName: this.envName,
                    sortOrder: this.sortOrder,
                    defaultToggleValue: this.defaultToggleValue
                }

                this.spinner = true;
                axios.post('api/FeatureToggles/AddEnvironment', param)
                    .then((response) => {
                        this.showSuccessAlert = true;
                        this.envName = ''
                        Bus.$emit("env-added")
                    }).catch((e) => {
                        window.alert(e)
                    }).finally(() => {
                        this.spinner = false;
                    });
            }
        },
        mounted() {
            Bus.$on("app-changed", app => {
                this.applicationId = app.id;
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

<style>

    .validationMessage {
        color: red
    }
</style>