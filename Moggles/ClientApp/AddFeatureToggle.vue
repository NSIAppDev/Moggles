<template>
	<div>
		<alert v-if="showSuccessAlert" :duration="alertDuration" type="success" @dismissed="showSuccessAlert = false">
			<p>
				<i class="fas fa-check-circle"></i> Feature toggle added.
			</p>
		</alert>

		<div class="panel-body">
			<div class="form-group">
				<div v-for="error in errors" :key="error" class="text-danger">{{error}}</div>
				<input class="form-control" v-model="featureToggleName" type="text" name="ftName" placeholder="Feature toggle name..." maxlength="80">
			</div>
			<div class="form-group">
				<input class="form-control" v-model="notes" type="text" name="ftNotes" placeholder="Notes..." maxlength="500">
			</div>
			<div class="form-group">
				<button :disabled="applicationId > 0? false : true" class="btn btn-default btn-primary" v-on:click="addFeatureToggle" type="button">Add</button>
			</div>
		</div>
	</div>
</template>

<script>
    import { Bus } from './event-bus'
    import axios from 'axios'

    export default {
        data() {
            return {
                applicationId: -1,
                notes: '',
                featureToggleName: "",
                errors: [],
                existingToggles: [],
                spinner: false,
                showSuccessAlert: false,
				alertDuration: 1500
            }
        },
        methods: {
            addFeatureToggle() {
                if (this.applicationId === -1)
                    return;

                this.errors = [];

                if (this.featureToggleName === "") {
                    this.errors.push("Feature toggle name cannot be empty")
                    return;
                };

                let param = {
                    applicationId: this.applicationId,
                    featureToggleName: this.featureToggleName,
                    notes: this.notes
                }
				
                Bus.$emit('block-ui')
                axios.post('api/FeatureToggles/addFeatureToggle', param)
                    .then((response) => {
                        this.showSuccessAlert = true;
                        this.featureToggleName = '';
                        this.notes = '';
                        Bus.$emit("toggle-added")
                    }).catch((e) => {                
                        this.errors.push(e.response.data);
                    }).finally(() => {
						Bus.$emit('unblock-ui')
                    });
            }
        },
        mounted() {
            Bus.$on("app-changed", app => {
                if (app) {
                    this.applicationId = app.id;
                }
            });

            Bus.$on("toggles-loaded", toggles => {
                this.existingToggles = toggles;
            });
        }
    }
</script>