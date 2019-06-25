<template>
	<div>
		<div v-if="showSuccessAlert" class="alert alert-success" @dismissed="showSuccessAlert = false">
			<p>
				<i class="fas fa-check-circle"></i> Application added succesfully.
			</p>
		</div>
		<div class="panel-body">
			<div class="form-group">
				<label>Application name:</label>
				<input class="form-control" v-model="applicationName" type="text" name="appName" placeholder="Application name..." maxlength="100">
			</div>
			<div class="form-group">
				<label>Add a first environment:</label>
				<div class="form-group">
					<input class="form-control" v-model="environmentName" type="text" name="envName" placeholder="Environment name..." maxlength="100">
					<div>
						<label>Default toggle value:</label>
						<div class="form-inline">
							<label for="d1">True</label>
							<input id="d1" v-model="defaultToggleValue" type="radio" name="defToggleValue" value="true" checked>

							<label for="d2">False</label>
							<input id="d2" v-model="defaultToggleValue" type="radio" name="defToggleValue" value="false">
						</div>
					</div>
				</div>
			</div>
			<div>
				<button class="btn btn-default btn-primary" v-on:click="addApplication" type="button">Add</button>
			</div>
			<div v-for="error in errors" :key="error" class="text-danger">{{error}}</div>
		</div>
	</div>
</template>

<script>
    import { Bus } from './event-bus'
    import axios from 'axios'

    export default {
        data() {
            return {
                applicationName: "",
                showSuccessAlert: false,
                environmentName: "",
                defaultToggleValue: false,
                errors: [],
				alertDuration: 1500
            }
        },
        methods: {
            addApplication() {
                this.errors = [];
                Bus.$emit('block-ui')

                if (this.applicationName === "") {
                    this.errors.push("Application name cannot be empty")
                }

                if (this.environmentName === "") {
                    this.errors.push("Environment name cannot be empty")
                }

                if (this.errors.length > 0) {
                    this.spinner = false;
                    return;
                }

                axios.post('api/Applications/add', {
                    applicationName: this.applicationName,
                    environmentName: this.environmentName,
                    defaultToggleValue: this.defaultToggleValue
                }).then((response) => {
                    this.applicationName = '';
                    this.environmentName = '';
                    this.defaultToggleValue = false;
                    this.showSuccessAlert = true;
					Bus.$emit("new-app-added");
					setTimeout(() => {
                        this.showSuccessAlert = false;
                    }, this.alertDuration)
                }).catch(e => {
                    window.alert(e)
                }).finally(e => {
					Bus.$emit('unblock-ui')
                });
            }
        }
    }
</script>