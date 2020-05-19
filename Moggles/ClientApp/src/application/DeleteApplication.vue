<template>
  <div>
    <div>
      Are you sure you want to delete the {{ application.appName }} application?
      <br>
      All associated feature toggles will be removed.
    </div>
    <br>
    <div class="text-right">
      <button type="button" class="btn btn-default" @click="cancel">
        Cancel
      </button>
      <button id="confirmDeleteApplicationBtn" type="button" class="btn btn-primary"
              @click="deleteApp">
        Delete
      </button>
    </div>
  </div>
</template>

<script>
    import { Bus } from '../common/event-bus'
    import axios from 'axios'
    import { events } from '../common/events';


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
                    Bus.$emit(events.refreshApplications);
                })
                    .catch(e => {
                        window.alert(e)
                    }).finally(() => {
                        Bus.$emit(events.unblockUI)
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