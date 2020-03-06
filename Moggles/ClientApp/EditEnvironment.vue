<template>
    <div>
        <div v-if="environmentToEdit" class="form-horizontal">
            <div class="row">
                <div class="col-sm-12">
                    <div v-for="error in editEnvErrors" :key="error" class="text-danger margin-left-15">
                        {{ error }}
                    </div>
                </div>
                <div class=" col-sm-12 form-group">
                    <label class="col-sm-4 control-label text-left">Environment name</label>
                    <div class="col-sm-8">
                        <input v-model="editedEnvironmentName" type="text" class="form-control">
                    </div>
                </div>
                <div class="col-sm-12 form-group">
                    <label class="col-sm-4 control-label">
                        Default value for new toggles
                    </label>
                    <div class="col-sm-6 margin-top-4">
                        <label for="r1">True</label>
                        <input id="r1" v-model="defaultToggleValue" type="radio"
                               :value="true" checked>

                        <label for="r2">False</label>
                        <input id="r2" v-model="defaultToggleValue" type="radio"
                               :value="false">
                    </div>
                </div>
                <div class="col-sm-12 form-group">
                    <label class="col-sm-4 control-label">
                        Require a reason when toggle state changes to
                    </label>
                    <div class="col-sm-6 margin-top-8">
                        <label for="reasonWhenEnabled">Enabled</label>
                        <p-check id="reasonWhenEnabled" v-model="requireReasonWhenToggleEnabled" class="p-icon p-fill"
                                 color="default">
                            <i slot="extra" class="icon fas fa-check" />
                        </p-check>
                        <label for="reasonWhenDisabled">Disabled</label>
                        <p-check id="reasonWhenDisabled" v-model="requireReasonWhenToggleDisabled" class="p-icon p-fill"
                                 color="default">
                            <i slot="extra" class="icon fas fa-check" />
                        </p-check>
                    </div>
                </div>
                <div class="clearfix">
                    <div class="col-sm-6">
                        <button type="button" class="btn btn-danger" @click="confirmDeleteEnvironment">
                            Delete
                        </button>
                    </div>
                    <div class="col-sm-6 text-right">
                        <button type="button" class="btn btn-default" @click="cancelEditEnvName">
                            Cancel
                        </button>
                        <button type="button" class="btn btn-primary" @click="saveEnvironment">
                            Save
                        </button>
                    </div>
                </div>
            </div>
        </div>
        <modal v-model="showDeleteEnvironmentConfirmation" title="You are about to delete an environment" :footer="false"
               append-to-body>
            <deleteEnvironment />
        </modal>
    </div>
</template>

<script>
    import axios from 'axios';
    import PrettyCheck from 'pretty-checkbox-vue/check';
    import { Bus } from './event-bus';
    import DeleteEnvironment from './DeleteEnvironment';
    import _ from 'lodash';

    export default {
        components: {
            'p-check': PrettyCheck,
            'deleteEnvironment': DeleteEnvironment
        },
        props: {
            application: {
                type: Object,
                required: true
            }
        },
        data() {
            return {
                environmentToEdit: null,
                showEditEnvironmentModal: false,
                editEnvErrors: [],
                editedEnvironmentName: "",
                environmentsEdited: [],
                environmentsToRefresh: [],
                requireReasonWhenToggleEnabled: false,
                requireReasonWhenToggleDisabled: false,
                defaultToggleValue: true,
                showDeleteEnvironmentConfirmation: false
            }
        },
        created() {
            Bus.$on('edit-environment', (environment) => {
                this.environmentToEdit = environment;
                this.loadData();
            });
            Bus.$on('delete-EnvironmentRefresh', envName => {
                let index = _.indexOf(this.environmentsToRefresh, envName);
                if (index != -1) {
                    this.environmentsToRefresh.splice(index, 1);
                }
            })
            Bus.$on('close-deleteEnvironment', () => {
                this.showDeleteEnvironmentConfirmation = false;
            })
        },

        methods: {
            loadData() {
                this.editedEnvironmentName = this.environmentToEdit.envName;
                this.defaultToggleValue = this.environmentToEdit.defaultToggleValue;
                this.requireReasonWhenToggleDisabled = this.environmentToEdit.requireReasonWhenToggleDisabled;
                this.requireReasonWhenToggleEnabled = this.environmentToEdit.requireReasonWhenToggleEnabled;
            },
            saveEnvironment() {
                this.editEnvErrors = []
                if (this.stringIsNullOrEmpty(this.editedEnvironmentName)) {
                    this.editEnvErrors.push("Environment name cannot be empty")
                    return;
                }

                let envUpdateModel = {
                    applicationId: this.application.id,
                    initialEnvName: this.environmentToEdit.envName,
                    newEnvName: this.editedEnvironmentName,
                    defaultToggleValue: this.defaultToggleValue,
                    requireReasonForChangeWhenToggleEnabled: this.requireReasonWhenToggleEnabled,
                    requireReasonForChangeWhenToggleDisabled: this.requireReasonWhenToggleDisabled
                }

                axios.put('/api/FeatureToggles/updateEnvironment', envUpdateModel)
                    .then(() => {
                        this.showEditEnvironmentModal = false;
                        this.environmentToEdit = null;
                        let index = _.indexOf(this.environmentsToRefresh, envUpdateModel.initialEnvName);
                        if (index != -1) {
                            this.environmentsToRefresh.splice(index, 1);
                        }
                        Bus.$emit('close-editEnvironment');
                    }).catch(error => window.alert(error))
            },
            confirmDeleteEnvironment() {
                this.showDeleteEnvironmentConfirmation = true
                Bus.$emit('delete-Environment', this.environmentToEdit.envName, this.application);
            },

            stringIsNullOrEmpty(text) {
                return !text || /^\s*$/.test(text);
            },
            cancelEditEnvName() {
                this.showEditEnvironmentModal = false
                this.environmentToEdit = null
                this.editFeatureToggleErrors = []
                Bus.$emit('close-editEnvironment');
            },
        }
    }
</script>