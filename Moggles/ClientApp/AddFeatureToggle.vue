<template>
  <div>
    <alert v-if="showSuccessAlert" :duration="alertDuration" type="success"
           @dismissed="showSuccessAlert = false">
      <p>
        <i class="fas fa-check-circle" /> Feature toggle added.
      </p>
    </alert>

    <div class="panel-body">
      <div v-for="error in errors" :key="error" class="text-danger">
        {{ error }}
      </div>
      <div class="form-group">
        <label for="ftname">Feature Toggle Name</label>
        <input v-model="featureToggleName" class="form-control" type="text"
               name="ftName" placeholder="Feature toggle name..." maxlength="80">
      </div>
      <div class="form-group">
        <label class="control-label" for="ftnotes">Notes</label>
        <input v-model="notes" class="form-control" type="text"
               name="ftNotes" placeholder="Notes..." maxlength="500">
      </div>
      <div class="form-group">
        <label class="control-label" for="ftPerm">Is Permanent </label>
        <span class="padding-left-5">
          <p-check v-model="isPermanent" class="p-icon p-fill" name="ftPerm"
                   color="default">
            <i slot="extra" class="icon fas fa-check" />
          </p-check>
        </span>
      </div>
      <div class="form-group">
        <div class="text-right">
          <button class="btn btn-default" @click="closeAddToggleModal">
            Close
          </button>
          <button :disabled="applicationId != ''? false : true" class="btn btn-primary" type="button"
                  @click="addFeatureToggle">
            Add
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
	import PrettyCheck from 'pretty-checkbox-vue/check';
	import { Bus } from './event-bus'
	import axios from 'axios'

	export default {
		components: {
			'p-check': PrettyCheck
		},
        data() {
			return {
				applicationId: -1,
				notes: '',
				featureToggleName: "",
				isPermanent: false,
				errors: [],
				existingToggles: [],
				spinner: false,
				showSuccessAlert: false,
				alertDuration: 1500
			}
        },
        computed() {
            this.clearFields();
        },
        mounted() {
            Bus.$on("app-changed", app => {
                if (app) {
                    this.applicationId = app.id;
                }
            });

            Bus.$on("toggles-loaded", toggles => {
                this.existingToggles = toggles;
            });
            Bus.$on("open-modal", () => {
                this.clearFields();
            })
		},
		methods: {
			addFeatureToggle() {
				if (this.applicationId === -1)
					return;

				this.errors = [];

				if (this.featureToggleName === "") {
					this.errors.push("Feature toggle name cannot be empty")
					return;
				}

				let param = {
					applicationId: this.applicationId,
					featureToggleName: this.featureToggleName,
					notes: this.notes,
					isPermanent: this.isPermanent
				}

				Bus.$emit('block-ui')
				axios.post('api/FeatureToggles/addFeatureToggle', param)
					.then(() => {
						this.showSuccessAlert = true;
						this.featureToggleName = '';
						this.notes = '';
						this.isPermanent = false;
						Bus.$emit("toggle-added")
					}).catch((e) => {
						this.errors.push(e.response.data);
                    }).finally(() => {
                        Bus.$emit('unblock-ui')
                    });    
			},
            closeAddToggleModal() {
                Bus.$emit('close-add-toggle');
            },
            clearFields() {
                this.featureToggleName = "";
                this.errors = [];
                this.notes = '';
                this.isPermanent = false;
            }
		}
	}
</script>