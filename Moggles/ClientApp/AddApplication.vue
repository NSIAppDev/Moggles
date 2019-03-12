<template>
    <div>
        <spinner ref="spinner" global v-model="spinner" size="lg" fixed></spinner>

        <div class="panel-body">
            <div class="form-group">
                <label>Application name:</label>
                <input class="form-control" v-model="applicationName" type="text" name="appName" placeholder="Application name..." maxlength="100">
            </div>
            <div class="form-group">
                <label>Add a first environment:</label>
                <div class="form-group">
                    <input class="form-control" v-model="environmentName" type="text" name="envName" placeholder="Environment name..." maxlength="100">
                    <div>
                        <label>Default toggle value:</label>
                        <div class="form-inline">
                            <label for="d1">True</label>
                            <input id="d1" v-model="defaultToggleValue" type="radio" name="defToggleValue" value="true" checked>

                            <label for="d2">False</label>
                            <input id="d2" v-model="defaultToggleValue" type="radio" name="defToggleValue" value="false">
                        </div>
                    </div>
                </div>
            </div>
            <div>
                <button class="btn btn-default btn-primary" v-on:click="addApplication" type="button">Add</button>
            </div>
            <div v-for="error in errors" :key="error" class="validationMessage">{{error}}</div>
        </div>
        <br />
        <div v-if="showSuccessAlert" class="alert alert-success" role="alert">Application added succesfully</div>
    </div>
</template>

<script>
    import { Bus } from './event-bus'
    import axios from 'axios'
    import { spinner } from 'vue-strap'

    export default {
        data() {
            return {
                applicationName: "",
                spinner: false,
                showSuccessAlert: false,
                environmentName: "",
                defaultToggleValue: false,
                errors: []
            }
        },
        methods: {
            addApplication() {
                this.errors = [];
                this.spinner = true;

                if (this.applicationName === "") {
                    this.errors.push("Application name cannot be empty")
                }

                if (this.environmentName === "") {
                    this.errors.push("Environment name cannot be empty")
                }

                if (this.errors.length > 0) {
                    this.spinner = false;
                    return;
                }

                axios.post('api/Applications/add', {
                    applicationName: this.applicationName,
                    environmentName: this.environmentName,
                    defaultToggleValue: this.defaultToggleValue
                }).then((response) => {
                    this.applicationName = '';
                    this.environmentName = '';
                    this.defaultToggleValue = false;
                    this.showSuccessAlert = true;
                    Bus.$emit("new-app-added");
                    setTimeout(() => {
                        this.showSuccessAlert = false;
                    }, 1500)
                }).catch(e => {
                    window.alert(e)
                }).finally(e => {
                    this.spinner = false;
                });
            }
        },
        components: {
            spinner
        }
    }
</script>