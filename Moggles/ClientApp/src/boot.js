import Vue from 'vue'
import App from './App.vue'
import VueGoodTablePlugin from 'vue-good-table'
import * as uiv from 'uiv'

Vue.use(VueGoodTablePlugin)
Vue.use(uiv)
Vue.use(require('vue-moment'))

new Vue({
    el: '#app-root',
    render: h => h(App)
})
