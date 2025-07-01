import { shallowMount } from '@vue/test-utils'
import AddApplication from '../src/application/AddApplication.vue'
import flushPromises from 'flush-promises'
import axios from 'axios';
import MockAdapter from 'axios-mock-adapter';
import sinon from 'sinon';
import { Bus } from '../src/common/event-bus'


describe('AddApplication.vue', () => {

    const mockAdapter = new MockAdapter(axios);

    beforeEach(() => {
        mockAdapter.onGet('/api/applications/assignedto-options').reply(200, []);
    });

    afterEach(() => {
        mockAdapter.reset();
        jest.useRealTimers();
    });

    test('Shows empty input on show', function () {
        const wrapper = shallowMount(AddApplication);
        const inputValue = wrapper.find('input').element.value;
        expect(inputValue).toBe("");
    })

    test('Application name is cleared on successfull add', async () => {
        const wrapper = shallowMount(AddApplication);
        wrapper.find('button').trigger('click');

        mockAdapter.onPost().reply(200);
        await flushPromises();

        expect(wrapper.vm.applicationName).toBe('');
    })

    test('A success alert is shown on successfull add and goes away after a while', async () => {

        jest.useFakeTimers();

        const wrapper = shallowMount(AddApplication);

        setInputValues(wrapper);

        triggerAdd(wrapper);

        mockAdapter.onPost().reply(200);
        await flushPromises();

        expect(wrapper.find('.alert').exists()).toBeTruthy();

        jest.advanceTimersByTime(1510);
        await wrapper.vm.$nextTick();

        expect(wrapper.find('.alert').exists()).toBeFalsy();

    })

    test('Emits app added event on succesfull Add', async () => {

        const spy = sinon.spy(Bus, '$emit');

        const wrapper = shallowMount(AddApplication);
        mockAdapter.onPost().reply(200);

        setInputValues(wrapper);

        triggerAdd(wrapper);
        await flushPromises();

        expect(spy.calledWithExactly('app-added', 'App')).toBe(true);

		spy.restore();
    })

    test('Calls the right URL passing the appName and environment name', async () => {

        const mock = sinon.mock(axios);
        mock.expects('post').withArgs('api/Applications/add', {
            applicationName: 'testApp',
            applicationAssignedTo: '',
            environmentName: "test",
            defaultToggleValue: true
        }).returns(Promise.resolve({}));

        const wrapper = shallowMount(AddApplication);

        wrapper.setData({
            applicationName: 'testApp',
            applicationAssignedTo: '',
            environmentName: "test",
            defaultToggleValue: true
        });

        triggerAdd(wrapper);

        await flushPromises();

        mock.verify();
        mock.restore();
    })

    test('Shows a spinner while request is in process', async () => {
		const wrapper = shallowMount(AddApplication);
        const spy = sinon.spy(Bus, '$emit');

        setInputValues(wrapper);

        triggerAdd(wrapper);
        mockAdapter.onPost().reply(200);

		expect(spy.calledWithExactly('block-ui')).toBe(true);

        await flushPromises();

		expect(spy.calledWithExactly('unblock-ui')).toBe(true);

		spy.restore();
    })

    function setInputValues(wrapper, appName = 'App', envName = 'Env') {
        wrapper.find('input[name="appName"]').setValue(appName);
        wrapper.find('input[name="envName"]').setValue(envName);
    }

    function triggerAdd(wrapper) {
        wrapper.find('button.btn-primary').trigger('click');
    }

})