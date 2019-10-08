<template>
  <div>
    <div v-if="showSuccessAlert" class="alert alert-success" @dismissed="showSuccessAlert = false">
      <p>
        <i class="fas fa-check-circle" /> Application added succesfully.
      </p>
    </div>
    <div class="panel-body">
      <div v-for="error in errors" :key="error" class="text-danger margin-bottom-10">
        {{ error }}
      </div>
      <div class="form-group">
        <label class="control-label" for="appname">Application name</label>
        <input v-model="applicationName" class="form-control" type="text"
               name="appName" placeholder="Application name..." maxlength="100">
      </div>
      <div class="form-group">
        <label class="control-label" for="envname">Add a first environment</label>
        <div class="form-group">
          <input v-model="environmentName" class="form-control" type="text"
                 name="envName" placeholder="Environment name..." maxlength="100">
        </div>
      </div>
      <div class="form-group">
        <label class="control-label">Default toggle value</label>
        <div class="form-inline">
          <label for="d1">
            <input id="d1" v-model="defaultToggleValue" type="radio"
                   :value="true"> True
          </label>
          <label for="d2">
            <input id="d2" v-model="defaultToggleValue" type="radio"
                   :value="false"> False
          </label>
        </div>
      </div>
      <div class="text-right">
        <button class="btn btn-default" @click="closeAddApplicationModal">
          Close
        </button>
        <button class="btn btn-primary" type="button" @click="addApplication">
          Add
        </button>
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
                applicationName: "",
                showSuccessAlert: false,
                environmentName: "",
                defaultToggleValue: true,
                errors: [],
				alertDuration: 1500
            }
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
					Bus.$emit('unblock-ui')
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
					Bus.$emit("new-app-added");
					setTimeout(() => {
                        this.showSuccessAlert = false;
                    }, this.alertDuration)
                }).catch(e => {
					this.errors.push(e.response.data);
                }).finally(() => {
					Bus.$emit('unblock-ui')
                });
            },
			closeAddApplicationModal() {
				Bus.$emit('close-add-application');
			}
        }
    }
</script>