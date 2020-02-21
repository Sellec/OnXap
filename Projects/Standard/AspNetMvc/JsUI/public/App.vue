<template>
    <div id="app">
        <Home msg="Hello world!" />
        <p-inputtext v-model="text"></p-inputtext><br />
        <input type="text" v-model="text" disabled />
    </div>
</template>

<script>
    import Vue from 'vue';
    import InputText from 'primevue/inputtext';
    import Button from 'primevue/button';
    import Toast from 'primevue/toast';
    import ToastService from 'primevue/toastservice';
    import Home from './components/Home.vue';

    Vue.use(ToastService);

    Vue.component('p-inputtext', InputText);
    Vue.component('p-button', Button);
    Vue.component('p-toast', Toast);

    export default {
        name: 'app',
        components: {
            Home
        },
        data: function () {
            return {
                text: ""
            }
        }
    };
</script>

<style>
</style>
