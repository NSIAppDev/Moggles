<template>
  <div>
    <vue-good-table id="deletedTogglesGrid" ref="deletedTogglesGrid"
                    :columns="gridColumns"
                    :rows="toggles"
                    :pagination-options="getPaginationOptions"
                    :sort-options="getSortOptions"
                    style-class="vgt-table striped condensed bordered">
      <div slot="emptystate">
        <div class="text-center">
          There are no deleted toggles for this application
        </div>
      </div>
    </vue-good-table>
  </div>
</template>
<script>
    import axios from 'axios';
    import moment from 'moment';
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
                gridColumns: [
                    {
                        field: 'toggleName',
                        label: 'Feature Toggle Name',
                        sortable: true,
                        thClass: 'sortable',
                        width: '200px',
                        filterOptions: {
                            enabled: true,
                            placeholder: 'Filter Toggle Name'
                        }
                    },
                    {
                        field: 'reason',
                        label: 'Reason',
                        width: '300px'
                    },
                    {
                        field: 'deletionDate',
                        label: 'Deleted',
                        sortable: true,
                        width: '140px',
                        thClass: 'sortable',
                        formatFn: this.formatDate,
                    }
                ],
                toggles: [],
                rowsPerPage: 10
            }
        },
        computed: {
            getPaginationOptions() {
                return { enabled: true, perPage: this.rowsPerPage, dropdownAllowAll: false };
            },
            getSortOptions() {
                return { enabled: true };
            }
        },
        mounted() {
            Bus.$on(events.showDeletedFeatureTogglesModal, () => {
                this.$refs['deletedTogglesGrid'].reset();
                this.getFeatureToggles();
            });
        },
        methods: {
            getFeatureToggles() {
                axios.get("/api/FeatureToggles/deletedFeatureToggles", {
                    params: {
                        applicationId: this.application.id
                    }
                }).then((response) => {
                    this.toggles = response.data;
                });
            },
            formatDate(date) {
                return moment(date).format('M/D/YY hh:mm:ss A');
            }
        }
    }
</script>