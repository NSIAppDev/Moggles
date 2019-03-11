import { shallowMount, mount } from '@vue/test-utils'
import ForceCacheRefresh from '../ForceCacheRefresh.vue'
import flushPromises from 'flush-promises'
import axios from 'axios';
//import MockAdapter from 'axios-mock-adapter';
import sinon from 'sinon';
import { Bus} from '../event-bus'


describe('ForceCacheRefresh.vue', () => {

    //let mockAdapter = new MockAdapter(axios);

    //beforeEach(() => {
    //    mockAdapter.reset();
    //});


   

    //it('A success alert is shown on successful add and goes await after a while',
    //    async () => {

    //        let clock = sinon.useFakeTimers();

    //        const wrapper = shallowMount(ForceCacheRefresh);
    //        wrapper.find('button').trigger('click');

    //        // mockAdapter.onPost().reply(200);
    //        let mock = sinon.mock(axios);
    //        mock.expects('post').returns(Promise.resolve({}));
    //        await flushPromises();

    //        expect(wrapper.find('.alert').exists()).toBe(true);
    //        clock.tick(1510);
    //        expect(wrapper.find('.alert').exists()).toBe(false);
    //        clock.restore();
    //        mock.restore();
    //    });

    it('Calls the right URL passing the app id and env name',
        async () => {

            let mock = sinon.mock(axios);
            mock.expects('post').withArgs('api/CacheRefresh', { applicationId: 13, envName: 'BAR' })
                .returns(Promise.resolve({}));
            const wrapper = shallowMount(ForceCacheRefresh);
            wrapper.setData({ applicationId: 13, existingEnvs: ['FOO', 'BAR'] });
            wrapper.find('#environmentSelect').setValue('BAR');
            wrapper.find('#refreshBtn').trigger('click');

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

    //it('Environment selection is cleared on successful refresh',
    //    async () => {
    //        const wrapper = shallowMount(ForceCacheRefresh);
    //        wrapper.find('button').trigger('click');

    //        // mockAdapter.onPost().reply(200);

    //        let mock = sinon.mock(axios);
    //        mock.expects('post').returns(Promise.resolve({}));

    //        await flushPromises();

    //        expect(wrapper.vm.envName).toBe('');
    //        mock.restore();
    //    });


    //it('Shows a spinner while request is in process', async () => {
    //    const wrapper = shallow(ForceCacheRefresh);
    //    wrapper.find('button').trigger('click');
    //    mockAdapter.onPost().reply(200);

    //    expect(wrapper.vm.spinner).toBe(true);

    //    await flushPromises();

    //    expect(wrapper.vm.spinner).toBe(false);
    //})

})