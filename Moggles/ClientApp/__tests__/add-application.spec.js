import { shallowMount } from '@vue/test-utils'
import AddApplication from '../src/application/AddApplication.vue'
import flushPromises from 'flush-promises'
import axios from 'axios';
import MockAdapter from 'axios-mock-adapter';
import sinon from 'sinon';
import { Bus } from '../src/common/event-bus'


describe('AddApplication.vue', () => {

    let mockAdapter = new MockAdapter(axios);

    beforeEach(() => {
        mockAdapter.reset();
    });

    test('Shows empty input on show', function () {
        const wrapper = shallowMount(AddApplication);
        let txt = wrapper.find('input').text();
        expect(txt).toBe("");
    })

    test('Application name is cleared on successful add', async () => {
        const wrapper = shallowMount(AddApplication);
        wrapper.find('button').trigger('click');

        mockAdapter.onPost().reply(200);
        await flushPromises();

        expect(wrapper.vm.applicationName).toBe('');
    })

    test('A success alert is shown on successful add and goes away after a while', async () => {

        jest.useFakeTimers();

        const wrapper = shallowMount(AddApplication);

        const appname = wrapper.find('input[name="appName"]');
        appname.setValue('App');

        const envname = wrapper.find('input[name="envName"]');
        envname.setValue('Env');

		wrapper.find('button.btn-primary').trigger('click');

        mockAdapter.onPost().reply(200);
        await flushPromises();

        expect(wrapper.find('.alert').exists()).toBeTruthy();

        jest.advanceTimersByTime(1510);
        await wrapper.vm.$nextTick();

        expect(wrapper.find('.alert').exists()).toBeFalsy();

    })

    test('Emits app added event on succesfull Add', async () => {

        let spy = sinon.spy(Bus, '$emit');

        const wrapper = shallowMount(AddApplication);
        mockAdapter.onPost().reply(200);

        const appname = wrapper.find('input[name="appName"]');
        appname.setValue('App');

        const envname = wrapper.find('input[name="envName"]');
        envname.setValue('Env');

		wrapper.find('button.btn-primary').trigger('click');
        await flushPromises();

        expect(spy.calledWithExactly('app-added')).toBe(true);

		spy.restore();
    })

    test('Calls the right URL passing the appName and environment name', async () => {

        let mock = sinon.mock(axios);
        mock.expects('post').withArgs('api/Applications/add', { applicationName: 'testApp', environmentName: "test", defaultToggleValue: true }).returns(Promise.resolve({}));
        const wrapper = shallowMount(AddApplication);
        wrapper.setData({ applicationName: 'testApp', environmentName: "test", defaultToggleValue: true });

		wrapper.find('button.btn-primary').trigger('click');

        await flushPromises();

        mock.verify();
        mock.restore();
    })

    test('Shows a spinner while request is in process', async () => {
		const wrapper = shallowMount(AddApplication);
		let spy = sinon.spy(Bus, '$emit');

        const appname = wrapper.find('input[name="appName"]');
        appname.setValue('App');

        const envname = wrapper.find('input[name="envName"]');
        envname.setValue('Env');

		wrapper.find('button.btn-primary').trigger('click');
        mockAdapter.onPost().reply(200);

		expect(spy.calledWithExactly('block-ui')).toBe(true);

        await flushPromises();

		expect(spy.calledWithExactly('unblock-ui')).toBe(true);

		spy.restore();
    })

})