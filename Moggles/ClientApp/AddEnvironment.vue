<template>
    <div>
        <alert v-if="showSuccessAlert" :duration="alertDuration" type="success" @dismissed="showSuccessAlert = false">
			<p>
				<i class="fas fa-check-circle"></i> Environment added.
			</p>
        </alert>

        <div class="panel-body">
            <div class="">
                <div class="form-group">
                    <div v-for="error in errors" :key="error" class="text-danger">{{error}}</div>
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

    export default {
        data() {
            return {
                applicationId: -1,
                envName: "",
                sortOrder: 500,
                defaultToggleValue: true,
                existingEnvs: [],
                errors: [],
				showSuccessAlert: false,
				alertDuration: 1500
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
				
                Bus.$emit('block-ui')
                axios.post('api/FeatureToggles/AddEnvironment', param)
                    .then((response) => {
                        this.showSuccessAlert = true;
                        this.envName = ''
                        Bus.$emit("env-added")
                    }).catch((e) => {
                        window.alert(e)
                    }).finally(() => {
						 Bus.$emit('unblock-ui')
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
        }
    }
</script>