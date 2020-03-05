<template>
    <div>
        <div v-if="toggleIsDeployed">
            <strong>{{ rowDataToDelete ? rowDataToDelete.toggleName: "" }}</strong> feature toggle is active on at least one environment. Are you sure you want to delete it?
        </div>
        <div v-else>
            Are you sure you want to delete this feature toggle?
        </div>
        <div class="text-right">
            <button type="button" class="btn btn-default" @click="cancelDeleteToggle">
                Cancel
            </button>
            <button type="button" class="btn btn-primary" @click="deleteToggle">
                Delete
            </button>
        </div>
    </div>
</template>
<script>
    import axios from 'axios';
    import { Bus } from './event-bus';
    export default {
        data() {
            return {
                toggleIsDeployed: false,
                rowDataToDelete: null,
                selectedAppId : null
            }
        },
        created() {
            Bus.$on('delete-featureToggle', toggleToDelete => {
                this.rowDataToDelete = toggleToDelete.toggle;
                this.selectedAppId = toggleToDelete.appId;
                this.toggleIsDeployed = this.isToggleActive(toggleToDelete);
            })
        },
        methods: {
            deleteToggle() {
                axios.delete(`/api/FeatureToggles?id=${this.rowDataToDelete.id}&applicationid=${this.selectedAppId}`).then(() => {
                    this.rowDataToDelete = null
                    this.toggleIsDeployed = false
                    Bus.$emit('close-deleteToggle');
                }).catch(error => window.alert(error))
            },
            isToggleActive(toggleData) {
                for (var propertyName in toggleData) {
                    if (propertyName.endsWith("_IsDeployed") && toggleData[propertyName] === true)
                        return true;
                }
                return false;
            },
            cancelDeleteToggle() {

                Bus.$emit('close-deleteToggle');
            }
        }
    }
</script>