import PrimeVue from 'primevue/config';
Vue.use(PrimeVue);

import Button from 'primevue/button';
import Editor from 'primevue/editor';
import InputText from 'primevue/inputtext';

export const NewsEdit = {
    ViewModel: null,
    Init: function () {
        return {
            components: {
                'pv-button': Button,
                'pv-editor': Editor,
                'pv-inputtext': InputText
            }
        };
    }
};
