<template>
  <div>
    <div>
      Are you sure you want to delete the {{this.application.appName}} application?
      <br>
      All associated feature toggles will be removed.
    </div>
    <br>
    <div class="text-right">
      <button type="button" class="btn btn-default" @click="cancel">
        Cancel
      </button>
      <button type="button" class="btn btn-primary" @click="deleteApp">
        Delete
      </button>
    </div>
  </div>
</template>

<script>
    import { Bus } from '../common/event-bus'
    import axios from 'axios'

    export default {
        props: {
            application: {
                type: Object,
                required: true
            }
        },
        methods: {
            deleteApp() {
                axios.delete(`/api/applications?id=${this.application.id}`).then(() => {
                    this.cancel();
                    this.deleteAppCompleted();
                    Bus.$emit("refresh-apps");    
                })
                .catch(e => {
                    window.alert(e)
                }).finally(() => {
                    Bus.$emit('unblock-ui')
                });
            },
            cancel() {
                this.$emit('cancel');
            },
            deleteAppCompleted() {
                this.$emit('deleteAppCompleted');
            }
        }
    }
</script>