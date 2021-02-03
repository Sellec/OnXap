import PrimeVue from 'primevue/config';
Vue.use(PrimeVue);

import Toast from 'primevue/toast';
import ToastService from 'primevue/toastservice';
Vue.use(ToastService);

export const Notifications = {
    Init: function () {
        new Vue({
            el: "#js-notification",
            components: { 
                'pv-toast': Toast
            },
            template: '<pv-toast position="top-right"></pv-toast>'
        });
    }
};