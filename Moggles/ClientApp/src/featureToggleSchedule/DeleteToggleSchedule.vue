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
    import { Bus } from '../common/event-bus';
    import axios from 'axios';
    import { events } from '../common/events';

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
                    Bus.$emit(events.closeDeleteSchedulerModal);
                    Bus.$emit(events.closeToggleSchedulerModal);
                }).catch(error => Bus.$emit(events.showErrorAlertModal, { 'error': error }));
            },
            closeModal() {
                Bus.$emit(events.closeDeleteSchedulerModal);
            }
        }
    }
</script>