<template>
  <div>
    <div>
      Are you sure you want to delete this feature toggle schedule?
    </div>
    <div class="text-right">
      <button type="button" class="btn btn-default" @click="closeModal">
        Cancel
      </button>
      <button type="button" class="btn btn-primary" @click="deleteSchedule">
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
            schedule: {
                type: Object,
                required: true
            }
        },
        methods: {
            deleteSchedule() {
                axios.delete(`/api/ToggleScheduler?id=${this.schedule.scheduleId}`).then(() => {
                    this.closeModal();
                }).catch(error => window.alert(error));
            },
            closeModal() {
                Bus.$emit('close-deleteScheduler');
            }
        }
    }
</script>