import { shallow, mount } from '@vue/test-utils'
import AddEnv from '../AddEnvironment'
import { alert } from 'vue-strap'

describe('AddEnvironment.vue', () => {
    test('is a Vue instance', () => {
        const wrapper = mount(AddEnv)
        expect(wrapper.isVueInstance()).toBeTruthy()
    })

    test('matches snapshot', () => {
        const wrapper = mount(AddEnv)
        expect(wrapper.vm.$el).toMatchSnapshot()
    })

})