<template>
  <div>
    <div>
      Are you sure you want to delete the environment?
      <br>
      All associated applications and feature toggles will be removed.
    </div>
    <div class="text-right">
      <button type="button" class="btn btn-default" @click="closeModal">
        Cancel
      </button>
      <button id="confirmDeleteEnvironmentBtn" type="button" class="btn btn-primary"
              @click="deleteEnvironment">
        Delete
      </button>
    </div>
  </div>
</template>

<script>
    import axios from 'axios';
    import { Bus } from '../common/event-bus';
    import { events } from '../common/events';


    export default {
        props: {
            application: {
                type: Object,
                required: true
            },
            environment: {
                type: Object,
                required: true
            }
        },
        methods: {
            deleteEnvironment() {
                let deleteEnvironmentModel = {
                    applicationId: this.application.id,
                    envName: this.environment.envName
                }

                axios.delete(`/api/FeatureToggles/environments`, { data: deleteEnvironmentModel }).then(() => {
                    Bus.$emit(events.applicationChanged, this.application)
                    Bus.$emit(events.closeDeleteEnvironmentModal);
                    Bus.$emit(events.closeEditEnvironmentModal);
                }).catch(error => Bus.$emit(events.showErrorAlertModal, { 'error': error }));
            },
            closeModal() {
                Bus.$emit(events.closeDeleteEnvironmentModal);
            }
        }
    }
</script>