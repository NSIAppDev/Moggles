<template>
  <div>
    <alert v-if="showSuccessAlert" :duration="alertDuration" type="success"
           @dismissed="showSuccessAlert = false">
      <p>
        <i class="fas fa-check-circle" /> Environment added.
      </p>
    </alert>

    <div class="panel-body">
      <div v-for="error in errors" :key="error" class="text-danger">
        {{ error }}
      </div>
      <div class="form-group">
        <label class="control-label" for="envname">Environment name</label>
        <input v-model="envName" class="form-control" type="text"
               name="envName" placeholder="Env name..." maxlength="50">
      </div>
      <div class="form-group">
        <label class="control-label">
          Default toggle value
        </label>
        <div>
          <label for="d1">True</label>
          <input id="d1" v-model="defaultToggleValue" type="radio"
                 :value="true" checked>

          <label for="d2">False</label>
          <input id="d2" v-model="defaultToggleValue" type="radio"
                 :value="false">
        </div>
      </div>
      <div class="text-right">
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
					.then(() => {
						this.showSuccessAlert = true;
						this.envName = '';
						this.defaultToggleValue = false;
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