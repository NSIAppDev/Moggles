<template>
    <div>
        <alert v-if="showSuccessAlert" :duration="alertDuration" type="success"
               @dismissed="showSuccessAlert = false">
            <p>
                <i class="fas fa-check-circle" /> Environment added.
            </p>
        </alert>

        <div class="form-horizontal">
            <div class="row">
                <div v-for="error in errors" :key="error" class="text-danger">
                    {{ error }}
                </div>
                <div class="col-sm-12 form-group">
                    <label class="col-sm-4 control-label" for="envname">Environment name</label>
                    <div class="col-sm-7">
                        <input ref="envName" v-model="envName" class="col-sm-8 form-control" type="text"
                               name="envName" placeholder="Env name..." maxlength="50">
                    </div>
                </div>
                <div class="col-sm-12 form-group">
                    <label class="col-sm-4 control-label">
                        Default toggle value
                    </label>
                    <div class="col-sm-6 margin-top-4">
                        <label for="b1">True</label>
                        <input id="b1" v-model="defaultToggleValue" type="radio"
                               :value="true" checked>

                        <label for="b2">False</label>
                        <input id="b2" v-model="defaultToggleValue" type="radio"
                               :value="false">
                    </div>
                </div>
                <div class="col-sm-12 text-right">
                    <button class="btn btn-default" @click="closeAddEnvironmentModal">
                        Close
                    </button>
                    <button :disabled="applicationId != ''? false : true" class="btn btn-primary" type="button"
                            @click="addEnv">
                        Add
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import { Bus } from './event-bus'
    import axios from 'axios'

    export default {
        data() {
            return {
                applicationId: -1,
                envName: "",
                sortOrder: 500,
                defaultToggleValue: false,
                existingEnvs: [],
                errors: [],
                showSuccessAlert: false,
                alertDuration: 1500
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

            Bus.$on('openAddEnvModal', () => {
                this.$nextTick(() => { this.$refs["envName"].focus() });
                this.clearFields();
            })
        },
        methods: {
            clearFields() {
                this.envName = "";
                this.errors = [];
                this.defaultToggleValue = false;
            },
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

                Bus.$emit('block-ui')
                axios.post('api/FeatureToggles/AddEnvironment', param)
                    .then(() => {
                        this.showSuccessAlert = true;
                        this.envName = '';
                        this.defaultToggleValue = false;
                        this.$nextTick(() => { this.$refs["envName"].focus() });
                        Bus.$emit("env-added")
                    }).catch((e) => {
                        window.alert(e)
                    }).finally(() => {
                        Bus.$emit('unblock-ui')
                    });
            },
            closeAddEnvironmentModal() {
                Bus.$emit('close-add-environment');
            }
        }
    }
</script>