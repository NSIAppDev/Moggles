import { mount } from '@vue/test-utils'
import AddEnvironment from '../src/environment/AddEnvironment.vue'


describe('AddEnvironment.vue', () => {
    test('is a Vue instance', () => {
        const wrapper = mount(AddEnvironment, {
            propsData: {
                application: {
                    id:1
                }
            }
        })
        expect(wrapper.findComponent(AddEnvironment).vm).toBeTruthy()
    })
})