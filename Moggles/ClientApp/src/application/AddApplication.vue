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
            <input id="addApplicationNameInput" ref="appName" v-model="applicationName"
                   class="form-control" type="text"
                   name="appName" placeholder="Application name..." maxlength="100">
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label" for="applicationAssignedTo">Assigned to</label>
          <div class="col-sm-8">
            <select id="assignedToDropdown" v-model="applicationAssignedTo" class="form-control" 
                    name="applicationAssignedTo">
              <option v-for="option in assignedToOptions" :key="option" :value="option">
                {{ option }}
              </option>
            </select>
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label" for="envname">Add a first environment</label>
          <div class="col-sm-8">
            <input id="addFirstEnvironmentInput" v-model="environmentName" class="form-control"
                   type="text"
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
          <button id="closeAddApplicationModalBtn" class="btn btn-default" @click="closeAddApplicationModal">
            Close
          </button>
          <button id="addApplicationBtn" class="btn btn-primary" type="button"
                  @click="addApplication">
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
                alertDuration: 1500,
                applicationAssignedTo: "None",
                assignedToOptions: []
            }
        },
        created() {
            axios.get('/api/applications/assignedto-options')
                .then(response => {
                    this.assignedToOptions = response.data;
                });
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
                    applicationAssignedTo: this.applicationAssignedTo,
                    environmentName: this.environmentName,
                    defaultToggleValue: this.defaultToggleValue
                }).then(() => {
                    Bus.$emit(events.newApplicationAdded, this.applicationName);
                    this.applicationName = '';
                    this.applicationAssignedTo = '';
                    this.environmentName = '';
                    this.defaultToggleValue = true;
                    this.showSuccessAlert = true;
                    this.$nextTick(() => { this.$refs["appName"].focus() });

                    
                    setTimeout(() => {
                        this.showSuccessAlert = false;
                    }, this.alertDuration)
                }).catch(error => {
                    Bus.$emit(events.showErrorAlertModal, { 'error': error })
                }).finally(() => {
                    Bus.$emit(events.unblockUI)
                });
                this.closeAddApplicationModal(); 
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