<template>
  <div>
    <div v-if="environment" class="form-horizontal">
      <div>
        <div class="col-sm-12">
          <div v-for="error in editEnvironmentErrors" :key="error" class="text-danger margin-left-15">
            {{ error }}
          </div>
        </div>
        <div class=" col-sm-12 form-group">
          <label class="col-sm-4 control-label text-left">Environment name</label>
          <div class="col-sm-8">
            <input id="editEnvironmentNameInput" v-model="environment.envName" type="text"
                   class="form-control">
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label">
            Default value for new toggles
          </label>
          <div class="col-sm-6 margin-top-4">
            <label for="r1">True</label>
            <input id="r1" v-model="environment.defaultToggleValue" type="radio"
                   :value="true" checked>

            <label for="r2">False</label>
            <input id="r2" v-model="environment.defaultToggleValue" type="radio"
                   :value="false">
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label">
            Require a reason when toggle state changes to
          </label>
          <div class="col-sm-6 margin-top-8">
            <label for="reasonWhenEnabled">Enabled</label>
            <p-check id="reasonWhenEnabled" v-model="environment.requireReasonWhenToggleEnabled" class="p-icon p-fill"
                     color="default">
              <i slot="extra" class="icon fas fa-check" />
            </p-check>
            <label for="reasonWhenDisabled">Disabled</label>
            <p-check id="reasonWhenDisabled" v-model="environment.requireReasonWhenToggleDisabled" class="p-icon p-fill"
                     color="default">
              <i slot="extra" class="icon fas fa-check" />
            </p-check>
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="control-label col-sm-4">Position</label>
          <div class="col-sm-6 margin-top-8">
            <a @click="leftMove=true"><i class="fas fa-fw fa-caret-left" /> <strong>Move Left</strong></a>
            <a class="pull-right" @click="rightMove=true"><strong>Move Right</strong> <i class="fas fa-fw fa-caret-right" /></a>
          </div>
        </div>
        <div class="clearfix">
          <div class="col-sm-6">
            <button id="deleteEnvironmentBtn" type="button" class="btn btn-danger"
                    @click="showDeleteEnvironmentConfirmationModal">
              Delete
            </button>
          </div>
          <div class="col-sm-6 text-right">
            <button type="button" class="btn btn-default" @click="cancelEditEnvironment">
              Cancel
            </button>
            <button id="saveEditEnvironmentBtn" type="button" class="btn btn-primary"
                    @click="saveEnvironment">
              Save
            </button>
          </div>
        </div>
      </div>
    </div>
    <modal v-model="showDeleteEnvironmentConfirmation" title="You are about to delete an environment" :footer="false"
           append-to-body>
      <deleteEnvironment :application="application" :environment="environment" />
    </modal>
  </div>
</template>

<script>
    import axios from 'axios';
    import PrettyCheck from 'pretty-checkbox-vue/check';
    import { Bus } from '../common/event-bus';
    import DeleteEnvironment from './DeleteEnvironment';
    import { events } from '../common/events';


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
                environment: {},
                initialEnvironmentName: '',
                editEnvironmentErrors: [],
                showDeleteEnvironmentConfirmation: false,
                leftMove: false,
                rightMove: false
            }
        },
        created() {
            Bus.$on(events.editEnvironment, environment => {
                this.initializeData();
                this.environment = environment;
                this.initialEnvironmentName = environment.envName;
            });

            Bus.$on(events.closeDeleteEnvironmentModal, () => {
                this.showDeleteEnvironmentConfirmation = false;
            })

        },
        methods: {
            initializeData() {
                this.rightMove = false;
                this.leftMove = false;
                this.showDeleteEnvironmentConfirmation = false;
                this.editEnvironmentErrors = [];
            },
            saveEnvironment() {
                this.editEnvironmentErrors = []

                if (this.stringIsNullOrEmpty(this.environment.envName)) {
                    this.editEnvironmentErrors.push("Environment name cannot be empty");
                    return;
                }

                let environmentUpdateModel = {
                    applicationId: this.application.id,
                    initialEnvName: this.initialEnvironmentName,
                    newEnvName: this.environment.envName,
                    sortOrder: this.environment.sortOrder,
                    defaultToggleValue: this.environment.defaultToggleValue,
                    requireReasonForChangeWhenToggleEnabled: this.environment.requireReasonWhenToggleEnabled,
                    requireReasonForChangeWhenToggleDisabled: this.environment.requireReasonWhenToggleDisabled,
                    moveToLeft: this.leftMove,
                    moveToRight: this.rightMove
                }

                axios.put('/api/FeatureToggles/updateEnvironment', environmentUpdateModel)
                    .then(() => {
                        Bus.$emit(events.closeEditEnvironmentModal);
                    }).catch(error => window.alert(error))
            },
            showDeleteEnvironmentConfirmationModal() {
                this.showDeleteEnvironmentConfirmation = true;
            },
            stringIsNullOrEmpty(text) {
                return !text || /^\s*$/.test(text);
            },
            cancelEditEnvironment() {
                Bus.$emit(events.closeEditEnvironmentModal);
            }
        }
    }
</script>

