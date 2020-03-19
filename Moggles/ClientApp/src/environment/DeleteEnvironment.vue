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
      <button type="button" class="btn btn-primary" @click="deleteEnvironment">
        Delete
      </button>
    </div>
  </div>
</template>

<script>
    import axios from 'axios';
    import { Bus } from '../common/event-bus';

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
                    Bus.$emit("app-changed", this.application)
                    Bus.$emit('close-deleteEnvironment');
                    Bus.$emit('close-editEnvironment');
                }).catch(error => window.alert(error))
            },
            closeModal() {
                Bus.$emit('close-deleteEnvironment');
            }
        }
    }
</script>