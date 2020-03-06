<template>
  <div>
    <div>
      Are you sure you want to delete this feature toggle schedule?
    </div>
    <div class="text-right">
      <button type="button" class="btn btn-default" @click="closeDialog">
        Cancel
      </button>
      <button type="button" class="btn btn-primary" @click="deleteScheduler">
        Delete
      </button>
    </div>
  </div>
</template>
<script>
    import { Bus } from './event-bus'
    import axios from 'axios'

    export default {
        data() {
            return {
                toggle: null
            }
        },
        created() {
            Bus.$on('delete-scheduler', toggle => {
                this.toggle = toggle;
            })
        },
        methods: {
            deleteScheduler() {
                axios.delete(`/api/ToggleScheduler?id=${this.toggle.id}`).then(() => {
                    Bus.$emit('close-deleteScheduler');
                    Bus.$emit('close-scheduler');
                }).catch(error => window.alert(error));
            },
            closeDialog() {
                Bus.$emit('close-deleteScheduler');
            }
        }
    }
</script>