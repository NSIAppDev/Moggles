import { shallowMount } from '@vue/test-utils'
import ForceCacheRefresh from '../src/menu/ForceCacheRefresh.vue'
import flushPromises from 'flush-promises'
import axios from 'axios';
import sinon from 'sinon';

describe('ForceCacheRefresh.vue', () => {
    
    it('Calls the right URL passing the app id and env name',
        async () => {
                
            let mock = sinon.mock(axios);
            mock.expects('post').withArgs('api/CacheRefresh', { applicationId: 13, envName: 'BAR' })
                .returns(Promise.resolve({}));
            const wrapper = shallowMount(ForceCacheRefresh);
            wrapper.setData({
                applicationId: 13, existingEnvs: [
                    {
                        id: '1',
                        envName: 'FOO',
                        defaultToggleValue: true,
                        sortOrder: 1
                    },
                    {
                        id: '1',
                        envName: 'BAR',
                        defaultToggleValue: true,
                        sortOrder: 1
                    }]
            });
            wrapper.find('#environmentSelect').setValue('BAR');
            wrapper.find('button.btn-primary').trigger('click');
            await flushPromises();


            mock.verify();
            mock.restore();

        });

    it('Shows empty select on show',
        function() {
            const wrapper = shallowMount(ForceCacheRefresh);
            let txt = wrapper.find('select').text();
            expect(txt).toBe("");
        });
})