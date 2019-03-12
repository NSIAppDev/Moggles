<template>
	<div>
		<vue-good-table ref="toggleGrid"
						:columns="gridColumns"
						:rows="toggles"
						:pagination-options="{
							enabled: true
						}"
						:sort-options="{
							enabled: true,
							initialSortBy: {field: 'toggleName', type: 'asc'}
						}"
						styleClass="vgt-table striped condensed bordered">
			<div slot="emptystate">
				<div class="text-center">There are no toggles for this application or filtered search</div>
			</div>
			<template slot="table-row" slot-scope="props">
				<span v-if="props.column.type == 'boolean'" :class="{ 'is-deployed': props.row[props.column.field + '_IsDeployed']}">
					<checkbox v-if="props.row[props.column.field + '_IsDeployed']" v-model="props.formattedRow[props.column.field]" type="success" disabled></checkbox>
					<checkbox v-if="!props.row[props.column.field + '_IsDeployed']" v-model="props.formattedRow[props.column.field]" disabled></checkbox>
				</span>
				<span v-else-if="props.column.field == 'id'">
					<a @click="edit(props.row)"><i class="fas fa-edit"></i></a>
					<a @click="confirmDelete(props.row)"><i class="fas fa-trash-alt"></i></a>
				</span>
				<span v-else-if="props.column.field == 'createdDate'">
					{{props.formattedRow.createdDate | moment('M/D/YY hh:mm:ss A')}}
				</span>
				<span v-else>
					{{props.formattedRow[props.column.field]}}
				</span>
			</template>
		</vue-good-table>
		<modal v-model="showEditModal" ok-text="Save" cancel-text="Cancel" :callback="saveToggle">
			<div slot="modal-header" class="modal-header">
				<h4 class="modal-title">Edit Feature Flags</h4>
			</div>
			<div slot="modal-body" class="mocdal-body">
				<div v-if="rowToEdit" class="form-horizontal">
					<div class="form-group" v-for="col in gridColumns">
						<div v-if="col.type == 'boolean'">
							<label class="col-sm-4 control-label">{{col.label}}</label>
							<div class="col-sm-8 margin-top-8">
								<div class="checkbox">
									<checkbox v-if="rowToEdit[col.field + '_IsDeployed']" v-model="rowToEdit[col.field]" type="success"></checkbox>
									<checkbox v-if="!rowToEdit[col.field + '_IsDeployed']" v-model="rowToEdit[col.field]"></checkbox>
								</div>
							</div>
						</div>
						<div v-else-if="col.field !== 'id' && col.field !== 'createdDate'">
							<div class="form-group">
								<label class="col-sm-4 control-label">{{col.label}}</label>
								<div class="col-sm-6">
									<input type="text" class="form-control" v-model="rowToEdit[col.field]">
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>
		</modal>
		<modal v-model="showDeleteConfirmation" ok-text="Delete" cancel-text="Cancel" :callback="deleteToggle">
			<div slot="modal-header" class="modal-header">
				<h4 class="modal-title">You are about to delete a feature toggle</h4>
			</div>
			<div slot="modal-body" class="modal-body">
				<div v-if="toggleIsDeployed">
					<strong>{{rowDataToDelete ? rowDataToDelete.toggleName: ""}}</strong> feature toggle is active on at least one environment. Are you sure you want to delete it?
				</div>
				<div v-else>
					Are you sure you want to delete this feature toggle?
				</div>
			</div>
		</modal>
	</div>
</template>
<script>
	import { modal, checkbox } from 'vue-strap'
	import axios from 'axios'
	import _ from 'lodash'
	import { Bus } from './event-bus'
	export default {
		environmentsList: [],
		components: {
			modal,
			checkbox
		},
		data() {
			const PAGE_SIZE = 15;

			return {
				toggles: [],
				gridColumns: [],
				selectedApp: {},
				rowToEdit: null,
				showEditModal: false,
				showAcceptedFeatures: false,
				showDeleteConfirmation: false,
				rowDataToDelete: null,
				toggleIsDeployed: false
			}
		},
		created() {
			Bus.$on("app-changed", app => {
				this.selectedApp = app;
				this.initializeGrid(app)
			})

			Bus.$on("env-added", () => {
				this.initializeGrid(this.selectedApp);
			})

			Bus.$on("toggle-added", () => {
				this.loadGridData(this.selectedApp.id)
			})
		},
		methods: {
			saveToggle() {
				let toggleUpdateModel = {
					id: this.rowToEdit.id,
					userAccepted: this.rowToEdit.userAccepted,
					notes: this.rowToEdit.notes,
					statuses: []
				}
				_.forEach(this.environmentsList, envName => {
					toggleUpdateModel.statuses.push({
						environment: envName,
						enabled: this.rowToEdit[envName]
					})
				});

				axios.put('/api/featuretoggles', toggleUpdateModel)
					.then((result) => {
						this.showEditModal = false
						this.rowToEdit = null
						this.loadGridData(this.selectedApp.id)
					}).catch(error => window.alert(error))
			},
			createGridColumns() {
				let columns = [
					{
						field: 'id',
						label: '',
						sortable: false,
						thClass: 'width-55'
					},
					{
						field: 'toggleName',
						label: 'Feature Toggle Name',
						sortable: true,
						thClass: 'sortable',
						filterOptions: {
							enabled: true,
							placeholder: 'Filter'
						}
					},
					{
						field: 'notes',
						label: 'Notes',
						sortable: true,
						thClass: 'sortable',
						filterOptions: {
							enabled: true,
							placeholder: 'Filter'
						}
					},
					{
						field: 'userAccepted',
						label: 'Accepted by User',
						type: 'boolean',
						sortable: false,
						filterOptions: {
							enabled: true,
							filterDropdownItems: [
								{ value: 'true', text: 'Accepted' },
								{ value: 'false', text: 'Unaccepted' }
							],
							filterValue: 'false',
							placeholder: 'Filter'
						}
					},
					{
						field: 'createdDate',
						label: 'Created',
						sortable: false,
					},
				]

				//create the environment columns
				columns.splice(2, 0, ..._.map(this.environmentsList, envName => {
					return {
						field: envName,
						label: envName,
						type: 'boolean',
						sortable: false,
						filterOptions: {
							enabled: true,
							filterDropdownItems: [
								{ value: 'true', text: 'Active' },
								{ value: 'false', text: 'Inactive' }
							],
							placeholder: 'Filter'
						},
					}
				}));


				this.gridColumns = columns
			},
			edit(row) {
				this.rowToEdit = row
				this.showEditModal = true
			},
			confirmDelete(row) {
				this.rowDataToDelete = row
				this.toggleIsDeployed = this.isToggleActive(this.rowDataToDelete)
				this.showDeleteConfirmation = true
			},
			deleteToggle() {
				axios.delete(`/api/FeatureToggles?id=${this.rowDataToDelete.id}`).then((result) => {
					this.showDeleteConfirmation = false
					this.rowDataToDelete = null
					this.toggleIsDeployed = false
					this.loadGridData(this.selectedApp.id)
				}).catch(error => window.alert(error))
			},
			isToggleActive(toggleData) {
				for (var propertyName in toggleData) {
					if (propertyName.endsWith("_IsDeployed") && toggleData[propertyName] === true)
						return true;
				}
				return false;
			},

			loadGridData(appId) {
				axios.get("/api/FeatureToggles", {
					params: {
						applicationId: appId
					}
				}).then((response) => {

					//create the flattened row models
					let gridRowModels = _.map(response.data, toggle => {
						let rowModel = {
							id: toggle.id,
							toggleName: toggle.toggleName,
							userAccepted: toggle.userAccepted,
							notes: toggle.notes,
							createdDate: new Date(toggle.createdDate)
						}

						this.environmentsList.forEach(env => {
							let envStatus = _.find(toggle.statuses, status => status.environment === env)
							rowModel[env] = envStatus ? envStatus.enabled : false;
							rowModel[env + '_IsDeployed'] = envStatus ? envStatus.isDeployed : false;
						});
						return rowModel;
					});

					this.toggles = gridRowModels;
					Bus.$emit('toggles-loaded', gridRowModels);

				}).catch(error => {
					//do not uncomment this, the null reference exception will return to haunt us !
					//window.alert(error)
				});
			},
			initializeGrid(app) {
				this.environmentsList = [];

				axios.get("/api/FeatureToggles/environments", {
					params: {
						applicationId: app.id
					}
				}).then((response) => {
					this.environmentsList = response.data;

					this.createGridColumns();
					this.loadGridData(app.id);
					this.$refs['toggleGrid'].reset()

					Bus.$emit('env-loaded', response.data)
				}).catch(error => window.alert(error));
			}

		}
	}
</script>
<style>
	.width-55 {
		width: 55px !important;
	}

	th.sortable, a {
		cursor: pointer;
	}

	.success > .dropdown-toggle {
		color: #fff;
		background-color: lightgreen !important;
		border-color: #398439 !important;
	}

		.success > .dropdown-toggle.btn-success {
			color: #fff;
			background-color: #449d44 !important;
			border-color: #398439 !important;
		}

	label.checkbox > .icon {
		top: -.5rem !important;
	}

	.margin-top-8 {
		margin-top: 8px;
	}
</style>