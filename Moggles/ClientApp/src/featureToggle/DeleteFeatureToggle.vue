<template>
  <div>
    <div v-if="isToggleDeployedOnAnyEnvironment">
      <strong>{{ toggleToDelete ? toggleToDelete.toggleName: "" }}</strong> feature toggle is active on at least one environment. Are you sure you want to delete it?
    </div>
    <div v-else>
      Are you sure you want to delete this feature toggle?
    </div>
    <div class="form-horizontal">
      <div class="col-sm-12">
        <h5 class="margin-top-20">
          <strong>Reason to delete</strong>
        </h5>
        <hr class="margin-top-1">
        <textarea v-model="reasonToChange" class="col-sm-12" rows="2" />
      </div>
    </div>
    <div class="text-right">
      <button type="button" class="btn btn-default margin-top-20" @click="cancelDeleteToggle">
        Cancel
      </button>
      <button id="deleteToggleBtn" type="button" class="btn btn-primary margin-top-20"
              @click="deleteToggle">
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
            }
        },
        data() {
            return {
                toggleToDelete: null
            }
        },
        computed: {
            isToggleDeployedOnAnyEnvironment() {
                for (var propertyName in this.toggleToDelete) {
                    if (propertyName.endsWith("_IsDeployed") && this.toggleToDelete[propertyName] === true)
                        return true;
                }
                return false;
            }
        },
        created() {
            Bus.$on(events.deleteFeatureToggle, toggleToDelete => {
                this.toggleToDelete = toggleToDelete;
            })
        },
        methods: {
            deleteToggle() {
                axios.delete(`/api/FeatureToggles?id=${this.toggleToDelete.id}&applicationid=${this.application.id}`).then(() => {
                    this.toggleToDelete = null
                    Bus.$emit(events.closeDeleteFeatureToggleModal);
                }).catch(error => Bus.$emit(events.showErrorAlertModal, { 'error': error }));
            },
            cancelDeleteToggle() {
                Bus.$emit(events.closeDeleteFeatureToggleModal);
            }
        }
    }
</script>