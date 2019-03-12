<template>
    <div>
        <spinner ref="spinner" v-model="spinner" size="sm"></spinner>

        <alert v-model="showSuccessAlert" placement="top-right" duration="1500" type="success" width="400px" dismissable>
            <span class="icon-ok-circled alert-icon-float-left"></span>
            <p>Feature toggle added.</p>
        </alert>

        <div class="panel-body">
            <div class="form-group">
                <div v-for="error in errors" :key="error" class="validationMessage">{{error}}</div>
                <input class="form-control" v-model="featureToggleName" type="text" name="ftName" placeholder="Feature toggle name..." maxlength="80">
            </div>
            <div class="form-group">
                <input class="form-control" v-model="notes" type="text" name="ftNotes" placeholder="Notes..." maxlength="500">
            </div>
            <div class="form-group">
                <button :disabled="applicationId > 0? false : true" class="btn btn-default btn-primary" v-on:click="addFeatureToggle" type="button">Add</button>
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
                notes: '',
                featureToggleName: "",
                errors: [],
                existingToggles: [],
                spinner: false,
                showSuccessAlert: false
            }
        },
        methods: {
            addFeatureToggle() {
                if (this.applicationId === -1)
                    return;

                this.errors = [];

                if (this.featureToggleName === "") {
                    this.errors.push("Feature toggle name cannot be empty")
                    return;
                };

                let param = {
                    applicationId: this.applicationId,
                    featureToggleName: this.featureToggleName,
                    notes: this.notes
                }

                this.spinner = true
                axios.post('api/FeatureToggles/addFeatureToggle', param)
                    .then((response) => {
                        this.showSuccessAlert = true;
                        this.featureToggleName = '';
                        this.notes = '';
                        Bus.$emit("toggle-added")
                    }).catch((e) => {                
                        this.errors.push(e.response.data);
                    }).finally(() => {
                        this.spinner = false;
                    });
            }
        },
        mounted() {
            Bus.$on("app-changed", app => {
                this.applicationId = app.id;
            });

            Bus.$on("toggles-loaded", toggles => {
                this.existingToggles = toggles;
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