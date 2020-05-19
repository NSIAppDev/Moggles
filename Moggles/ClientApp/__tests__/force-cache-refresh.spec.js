import { shallowMount } from '@vue/test-utils'
import ForceCacheRefresh from '../src/menu/ForceCacheRefresh.vue'
import flushPromises from 'flush-promises'
import axios from 'axios';
import sinon from 'sinon';


describe('ForceCacheRefresh.vue', () => {
    
    test('Calls the right URL passing the app id and env name',
        async () => {
                
            let mock = sinon.mock(axios);
            mock.expects('post').withArgs('api/CacheRefresh', { applicationId: 13, envName: 'BAR' })
                .returns(Promise.resolve({}));


            const wrapper = shallowMount(ForceCacheRefresh, {
                stubs: {
                    'alert': '<div id="alert"></div>'
                }
            });
            wrapper.setData({
                applicationId: 13, existingEnvs: [
                    {
                        id: '1',
                        envName: 'FOO',
                        defaultToggleValue: true,
                        sortOrder: 1
                    },
                    {
                        id: '2',
                        envName: 'BAR',
                        defaultToggleValue: true,
                        sortOrder: 2
                    }]
            });
            await wrapper.vm.$nextTick();

            wrapper.findAll('#environmentSelect > option').at(1).element.selected = true;
            wrapper.find('#environmentSelect').trigger('change');
            await wrapper.vm.$nextTick();


            wrapper.find('#refreshBtn').trigger('click');

            await wrapper.vm.$nextTick();

            await flushPromises();

            mock.verify();
            mock.restore();

        });

    test('Shows empty select on show',
        function() {
            const wrapper = shallowMount(ForceCacheRefresh, {
                stubs: {
                    'alert': '<div id="alert"></div>'
                }
            });
            let txt = wrapper.find('select').text();
            expect(txt).toBe("");
        });
})