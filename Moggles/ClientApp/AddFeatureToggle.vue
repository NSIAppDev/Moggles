<template>
  <div>
    <alert v-if="showSuccessAlert" :duration="alertDuration" type="success"
           @dismissed="showSuccessAlert = false">
      <p>
        <i class="fas fa-check-circle" /> Feature toggle added.
      </p>
    </alert>

    <div class="form-horizontal">
      <div class="row">
        <div v-for="error in errors" :key="error" class="text-danger">
          {{ error }}
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label" for="ftname">Name</label>
          <div class="col-sm-8">
            <input id="featureToggleName" ref="toggleName" v-model="featureToggleName"
                   class="form-control" type="text"
                   name="ftName" placeholder="Feature toggle name..." maxlength="80"
                   autoFocus>
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label" for="ftWorkItem">Work Item ID</label>
          <div class="col-sm-8">
            <input v-model="workItemIdentifier" class="form-control" type="text"
                   name="ftWorkItem" placeholder="Work Item ID..." maxlength="50">
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label" for="ftnotes">Notes</label>
          <div class="col-sm-8">
            <input v-model="notes" class="form-control" type="text"
                   name="ftNotes" placeholder="Notes..." maxlength="500">
          </div>
        </div>
        <div class="col-sm-12 form-group">
          <label class="col-sm-4 control-label" for="ftPerm">Is Permanent </label>
          <span class="col-sm-2 margin-top-5">
            <p-check v-model="isPermanent" class="p-icon p-fill" name="ftPerm"
                     color="default">
              <i slot="extra" class="icon fas fa-check" />
            </p-check>
          </span>
        </div>
        <div class="col-sm-12 text-right">
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
                alertDuration: 1500,
                workItemIdentifier: ""
            }
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
            Bus.$on("openAddFeatureToggleModal", () => {
                this.$nextTick(() => { this.$refs["toggleName"].focus() });
                this.clearFields();
            });

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
                    isPermanent: this.isPermanent,
                    workItemIdentifier: this.workItemIdentifier.trim()
                }

                Bus.$emit('block-ui')
                axios.post('api/FeatureToggles/addFeatureToggle', param)
                    .then(() => {
                        this.showSuccessAlert = true;
                        this.featureToggleName = '';
                        this.notes = '';
                        this.isPermanent = false;
                        this.workItemIdentifier = "";
                         this.$nextTick(() => { this.$refs["toggleName"].focus() });
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
                this.workItemIdentifier = "";
                this.isPermanent = false;
            }
        }
    }
</script>