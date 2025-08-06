<template>
	<div>
		<alert v-if="showRefreshAlert" type="info">
			<button type="button" class="close" @click="closeRefreshAlert">
				<span>×</span>
			</button>
			<h4>Toggles Have Been Modified, would you like to refresh the environments?</h4>
			<span v-for="(env, index) in environmentsToRefresh" :key="env" class="env-button">
				<button id="refreshEnvironmentsBtn" class="btn btn-default text-uppercase" @click="refreshEnvironmentToggles(env, index)"><strong>{{ env }}</strong></button>
			</span>
		</alert>
	</div>
</template>

<script>
    import { Bus } from './event-bus';
    import { events } from './events';
    import axios from 'axios';

	export default {
        props: {
            showRefreshAlert: {
                type: Boolean,
                required: true
            },
            environmentsToRefresh: {
                type: Array,
                required: true
            },
            selectedApp: {
                type: Object,
                required: true
            }
        },
		methods: {
            refreshEnvironmentToggles(env, index) {
                if (!this.selectedApp)
                    return;

                Bus.$emit(events.blockUI)

                axios.post('api/CacheRefresh', {
                    applicationId: this.selectedApp.id,
                    envName: env
                })
                    .then(() => {
                        this.environmentsToRefresh.splice(index, 1);
                        ///shouldn't need the below code, but computed value doesn't register the length as 0 without it
                        if (this.environmentsToRefresh.length === 0) {
                            this.environmentsToRefresh = [];
                        }

                        this.$notify({
                            type: 'success',
                            content: `${env} Cache Refreshed.`,
                            offsetY: 70,
                            icon: 'fas fa-check-circle'
                        })
                    }).catch(error => {
                        Bus.$emit(events.showErrorAlertModal, { 'error': error })
                    }).finally(() => {
                        Bus.$emit(events.unblockUI)
                    });
            },
            closeRefreshAlert() {
                this.isRefreshAlertVisible = false;
            }
		}
	}
</script>