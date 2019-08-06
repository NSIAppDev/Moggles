<template>
	<div>
		<alert v-if="showSuccessAlert" :duration="alertDuration" type="success" @dismissed="showSuccessAlert = false">
			<p>
				<i class="fas fa-check-circle"></i> Cache Refreshed.
			</p>
		</alert>

		<div class="panel-body">
			<div class="form-group">
				<label>Select environment for which to refresh the cache:</label>
				<select class="form-control" v-model="envName" required id="environmentSelect">
					<option v-for="env in existingEnvs">{{ env }}</option>
				</select>
			</div>
			<div class="text-right">
				<button class="btn btn-default" @click="closeRefreshModal">Close</button>
				<button id="refreshBtn" :disabled="applicationId > 0 && envName ? false : true" class="btn btn-primary" v-on:click="refresh" type="button">Refresh</button>
			</div>
		</div>
	</div>
</template>

<script>
	import { Bus } from './event-bus';
	import axios from 'axios';

	export default {
		data() {
			return {
				applicationId: -1,
				existingEnvs: [],
				spinner: false,
				showSuccessAlert: false,
				envName: null,
				alertDuration: 1500
			};
		},
		methods: {
			refresh() {
				if (this.applicationId === -1)
					return;

				let param = {
					applicationId: this.applicationId,
					envName: this.envName
				};

				Bus.$emit('block-ui')
				axios.post('api/CacheRefresh', param)
					.then((response) => {
						this.showSuccessAlert = true;
						this.envName = null;
					}).catch((e) => {
						window.alert(e);
					}).finally(() => {
						Bus.$emit('unblock-ui')
					});
			},
			closeRefreshModal() {
				Bus.$emit('close-refresh');
			}
		},
		mounted() {
			Bus.$on("app-changed", app => {
				if (app) {
					this.applicationId = app.id;
				}
			});

			Bus.$on("env-loaded", envs => {
				this.existingEnvs = envs;
			});
		}
	}
</script>
