<template>    
    <div>
        <h4>{{getMessage}}</h4>
        <div v-if="getTechnicalDetails">
            <button id="viewTechnicalDetailsBtn" class="btn btn-default errorDetailsBtn" @click="toggleDetails">
                {{getDetailsButtonName}}
            </button>
            <div v-if="showTechnicalDetails">
                <div class="panel panel-default">
                    <div class="panel-body">
                        <div class="errorDetailsBox"> 
                            {{getTechnicalDetails}}
                        </div>                       
                    </div>
                </div>            
            </div>
        </div>
        <div class="row">
            <div class="col-sm-12 text-right">
                <button id="closeErrorAlertBtn" class="btn btn-primary" @click="cancel">
                    Close
                </button>
            </div>
        </div>
    </div>
</template>
<script>
    import { Bus } from '../common/event-bus'
    import { events } from '../common/events';

    export default {
        data() {
            return {
                showTechnicalDetails: false
            }
        },
        props: {
            error: {
                type: Error,
                default: ''
            },
            customErrorMessage: {
                type: String,
                default: null
            }
        },
         created() {
             Bus.$on(events.showErrorAlertModal, () => {
                 this.reset();
            });
        },
        methods: {
            cancel() {
                this.$emit('cancel');
            },
            toggleDetails() {
                this.showTechnicalDetails = !this.showTechnicalDetails;
            },
            reset() {
                this.showTechnicalDetails = false;
            }
        },
        computed: {
            getMessage() {
                return this.customErrorMessage ? this.customErrorMessage : this.error.response.status + ' - ' + this.error.response.statusText;
            },
            getTechnicalDetails() {
                return this.error ? this.error.response.data : null;
            },
            getDetailsButtonName() {
                return this.showTechnicalDetails ? "Hide Technical Details" : "View Technical Details";
            }
        }
    }
</script>

