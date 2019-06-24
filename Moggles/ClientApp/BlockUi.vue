<template>
	<div>
		<transition name="block-fade">
			<div class="block-backdrop" v-if="block">
				<div class="block-message">
					<i class="fas fa-spinner fa-pulse fa-5x"></i>
				</div>
			</div>
		</transition>
	</div>
</template>

<script>
	import { Bus } from './event-bus'
	export default {
		data() {
			return {
				block: false
			}
		},
		mounted() {
			Bus.$on('block-ui', () => this.blockUI())
			Bus.$on('unblock-ui', () => this.unblockUI())
		},
		methods: {
			blockUI() {
				this.block = true
			},
			unblockUI() {
				this.block = false
			}
		}
	}
</script>