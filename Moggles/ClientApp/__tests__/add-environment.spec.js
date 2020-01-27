import { mount } from '@vue/test-utils'
import AddEnv from '../AddEnvironment'


describe('AddEnvironment.vue', () => {
    test('is a Vue instance', () => {
        const wrapper = mount(AddEnv)
        expect(wrapper.isVueInstance()).toBeTruthy()
    })
})