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
            <input ref="envName" v-model="envName" class="col-sm-8 form-control"
                   type="text"
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
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label">
            Require a reason when toggle state changes to 
          </label>
          <div class="col-sm-6 margin-top-4">
            <label for="requireReasonWhenToggleEnabled">Enabled</label>
            <p-check v-model="requireReasonWhenToggleEnabled" class="p-icon p-fill" color="default">
              <i slot="extra" class="icon fas fa-check" />
            </p-check>
            <label for="requireReasonWhenToggleDisabled">Disabled</label>
            <p-check v-model="requireReasonWhenToggleDisabled" class="p-icon p-fill" color="default">
              <i slot="extra" class="icon fas fa-check" />
            </p-check>
          </div>
        </div>
        <div class="col-sm-12 text-right">
          <button class="btn btn-default" @click="closeAddEnvironmentModal">
            Close
          </button>
          <button :disabled="application.id != ''? false : true" class="btn btn-primary" type="button"
                  @click="addEnv">
            Add
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
    import { Bus } from '../common/event-bus'
    import axios from 'axios'
    import PrettyCheck from 'pretty-checkbox-vue/check'

    export default {
        components: {
            'p-check': PrettyCheck
        },
        props: {
            application: {
                type: Object,
                required: true
            }
        },
        data() {
            return {
                envName: "",
                sortOrder: 500,
                defaultToggleValue: false,
                existingEnvs: [],
                errors: [],
                showSuccessAlert: false,
                alertDuration: 1500,
                requireReasonWhenToggleEnabled: false,
                requireReasonWhenToggleDisabled:false
            }
        },
        mounted() {
            Bus.$on("env-loaded", envs => {
                this.existingEnvs = envs;
            });

            this.$nextTick(() => { this.$refs["envName"].focus() });
            this.clearFields();
        },
        methods: {
            clearFields() {
                this.envName = "";
                this.errors = [];
                this.defaultToggleValue = false;
            },
            addEnv() {
                if (this.application.id === -1)
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
                    applicationId: this.application.id,
                    envName: this.envName,
                    sortOrder: this.sortOrder,
                    defaultToggleValue: this.defaultToggleValue,
                    requireReasonToChangeWhenToggleEnabled: this.requireReasonWhenToggleEnabled,
                    requireReasonToChangeWhenToggleDisabled: this.requireReasonWhenToggleDisabled
                }

                Bus.$emit('block-ui')
                axios.post('api/FeatureToggles/AddEnvironment', param)
                    .then(() => {
                        this.showSuccessAlert = true;
                        this.envName = '';
                        this.defaultToggleValue = false;
                        this.requireReasonWhenToggleEnabled = false;
                        this.requireReasonWhenToggleDisabled = false;
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