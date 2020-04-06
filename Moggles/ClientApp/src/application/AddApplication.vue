<template>
  <div>
    <div v-if="showSuccessAlert" class="alert alert-success" @dismissed="showSuccessAlert = false">
      <p>
        <i class="fas fa-check-circle" /> Application added succesfully.
      </p>
    </div>
    <div class="form-horizontal">
      <div class="row">
        <div v-for="error in errors" :key="error" class="text-danger margin-bottom-10">
          {{ error }}
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label" for="appname">Application name</label>
          <div class="col-sm-8">
            <input ref="appName" v-model="applicationName" class="form-control"
                   type="text" id="testingAddApplicationName"
                   name="appName" placeholder="Application name..." maxlength="100">
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label" for="envname">Add a first environment</label>
          <div class="col-sm-8">
            <input v-model="environmentName" class="form-control" type="text" id="testingAddFirstEnvironment"
                   name="envName" placeholder="Environment name..." maxlength="100">
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label">
            Default toggle value
          </label>
          <div class="col-sm-6 margin-top-4">
            <label for="d1">True</label>
            <input id="d1" v-model="defaultToggleValue" type="radio"
                   :value="true" checked>

            <label for="d2">False</label>
            <input id="d2" v-model="defaultToggleValue" type="radio"
                   :value="false">
          </div>
        </div>
        <div class="col-sm-12 text-right">
          <button class="btn btn-default" @click="closeAddApplicationModal" id="testingCloseAddApplicationModal">
            Close
          </button>
          <button class="btn btn-primary" type="button" id="testingAddApplication" @click="addApplication">
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
    import { events } from '../common/events';

    export default {
        data() {
            return {
                applicationName: "",
                showSuccessAlert: false,
                environmentName: "",
                defaultToggleValue: true,
                errors: [],
                alertDuration: 1500
            }
        },
        mounted() {
            Bus.$on(events.openAddApplicationModal, () => {
                this.$nextTick(() => { this.$refs["appName"].focus() });
                this.clearFields();
            })
        },
        methods: {
            addApplication() {
                this.errors = [];
                Bus.$emit('block-ui')

                if (this.applicationName === "") {
                    this.errors.push("Application name cannot be empty")
                }

                if (this.environmentName === "") {
                    this.errors.push("Environment name cannot be empty")
                }

                if (this.errors.length > 0) {
                    Bus.$emit(events.unblockUI);
                    return;
                }

                axios.post('api/Applications/add', {
                    applicationName: this.applicationName,
                    environmentName: this.environmentName,
                    defaultToggleValue: this.defaultToggleValue
                }).then(() => {
                    this.applicationName = '';
                    this.environmentName = '';
                    this.defaultToggleValue = true;
                    this.showSuccessAlert = true;
                    this.$nextTick(() => { this.$refs["appName"].focus() });

                    Bus.$emit(events.newApplicationAdded);
                    setTimeout(() => {
                        this.showSuccessAlert = false;
                    }, this.alertDuration)
                }).catch(e => {
                    this.errors.push(e.response.data);
                }).finally(() => {
                    Bus.$emit(events.unblockUI)
                });
            },
            closeAddApplicationModal() {
                Bus.$emit(events.closeAddApplicationModal);
            },
            clearFields() {
                this.applicationName = "";
                this.environmentName = "";
                this.errors = [];
                this.defaultToggleValue = true;
            }
        }
    }
</script>