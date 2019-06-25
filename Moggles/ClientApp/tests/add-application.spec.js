import { shallowMount, mount } from '@vue/test-utils'
import AddApplication from '../AddApplication.vue'
import flushPromises from 'flush-promises'
import axios from 'axios';
import MockAdapter from 'axios-mock-adapter';
import sinon from 'sinon';
import { Bus} from '../event-bus'


describe('AddApplication.vue', () => {

    let mockAdapter = new MockAdapter(axios);

    beforeEach(() => {
        mockAdapter.reset();
    });

    it('Shows empty input on show', function () {
        const wrapper = shallowMount(AddApplication);
        let txt = wrapper.find('input').text();
        expect(txt).toBe("");
    })

    it('Application name is cleared on successful add', async () => {
        const wrapper = shallowMount(AddApplication);
        wrapper.find('button').trigger('click');

        mockAdapter.onPost().reply(200);
        await flushPromises();

        expect(wrapper.vm.applicationName).toBe('');
    })

    it('A success alert is shown on successful add and goes await after a while', async () => {

        let clock = sinon.useFakeTimers();

        const wrapper = shallowMount(AddApplication);

        const appname = wrapper.find('input[name="appName"]');
        appname.setValue('App');

        const envname = wrapper.find('input[name="envName"]');
        envname.setValue('Env');

        wrapper.find('button').trigger('click');

        mockAdapter.onPost().reply(200);
        await flushPromises();

        expect(wrapper.find('.alert').exists()).toBe(true);
        clock.tick(1510);
        expect(wrapper.find('.alert').exists()).toBe(false);
        clock.restore();
    })

    it('Emits app added event on succesfull Add', async () => {

        let spy = sinon.spy(Bus, '$emit');

        const wrapper = shallowMount(AddApplication);
        mockAdapter.onPost().reply(200);

        const appname = wrapper.find('input[name="appName"]');
        appname.setValue('App');

        const envname = wrapper.find('input[name="envName"]');
        envname.setValue('Env');

        wrapper.find('button').trigger('click');
        await flushPromises();

		expect(spy.calledWithExactly('new-app-added')).toBe(true);

		spy.restore();
    })

    it('Calls the right URL passing the appName and environment name', async () => {

        let mock = sinon.mock(axios);
        mock.expects('post').withArgs('api/Applications/add', { applicationName: 'testApp', environmentName: "test", defaultToggleValue: true }).returns(Promise.resolve({}));
        const wrapper = shallowMount(AddApplication);
        wrapper.setData({ applicationName: 'testApp', environmentName: "test", defaultToggleValue: true });

        wrapper.find('button').trigger('click');

        await flushPromises();

        mock.verify();
    })

    it('Shows a spinner while request is in process', async () => {
		const wrapper = shallowMount(AddApplication);
		let spy = sinon.spy(Bus, '$emit');

        const appname = wrapper.find('input[name="appName"]');
        appname.setValue('App');

        const envname = wrapper.find('input[name="envName"]');
        envname.setValue('Env');

        wrapper.find('button').trigger('click');
        mockAdapter.onPost().reply(200);

		expect(spy.calledWithExactly('block-ui')).toBe(true);

        await flushPromises();

		expect(spy.calledWithExactly('unblock-ui')).toBe(true);

		spy.restore();
    })

})