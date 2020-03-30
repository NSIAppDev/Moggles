<template>
  <div>
    <alert v-if="showSuccessAlert" :duration="alertDuration" type="success"
           @dismissed="showSuccessAlert = false">
      <p>
        <i class="fas fa-check-circle" /> Cache Refreshed.
      </p>
    </alert>

    <div class="panel-body">
      <div class="form-group">
        <label>Select environment for which to refresh the cache:</label>
        <select id="environmentSelect" v-model="envName" class="form-control"
                required>
          <option v-for="env in existingEnvs" :key="env.envName">
            {{ env }}
          </option>
        </select>
      </div>
      <div class="text-right">
        <button class="btn btn-default" @click="closeRefreshModal">
          Close
        </button>
        <button id="refreshBtn" :disabled="applicationId != '' && envName ? false : true" class="btn btn-primary"
                type="button" @click="refresh">
          Refresh
        </button>
      </div>
    </div>
  </div>
</template>

<script>
	import { Bus } from '../common/event-bus';
	import axios from 'axios';
	import { events } from '../common/events';

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
		mounted() {
			Bus.$on(events.applicationChanged, app => {
				if (app) {
					this.applicationId = app.id;
				}
			});

			Bus.$on(events.environmentsLoaded, envs => {
				this.existingEnvs = envs;
			});
		},
		methods: {
			refresh() {
				if (this.applicationId === -1)
					return;

				let param = {
					applicationId: this.applicationId,
					envName: this.envName
				};

                Bus.$emit(events.blockUI)
				axios.post('api/CacheRefresh', param)
					.then(() => {
						this.showSuccessAlert = true;
						this.envName = null;
					}).catch((e) => {
						window.alert(e);
                    }).finally(() => {
                        Bus.$emit(events.unblockUI)
					});
			},
			closeRefreshModal() {
				Bus.$emit(events.closeForceCacheRefreshModal);
			}
		}
	}
</script>
