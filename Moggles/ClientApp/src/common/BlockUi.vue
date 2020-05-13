<template>
  <div>
    <transition name="block-fade">
      <div v-if="block" class="block-backdrop">
        <div class="block-message">
          <i class="fas fa-spinner fa-pulse fa-5x" />
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