import Vue from 'vue'
import App from './App.vue'
import VueGoodTablePlugin from 'vue-good-table'
import 'bootstrap'

Vue.use(VueGoodTablePlugin)
Vue.use(require('vue-moment'))

new Vue({
    el: '#app-root',
    render: h => h(App)
})
