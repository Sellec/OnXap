//import Vue from 'vue'; // не уверен, что Vue.esm надо включать в сборку.
import Button from 'primevue/button';
import Calendar from 'primevue/calendar';
import InputText from 'primevue/inputtext';

import Toast from 'primevue/toast';
import ToastService from 'primevue/toastservice';

import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import ColumnGroup from 'primevue/columngroup';

import 'primevue/resources/primevue.css';
import 'primeicons/primeicons.css';

var pTagsInternal = {
    'pvl-inputtext': InputText,
    'pvl-button': Button,
    'pvl-calendar': Calendar,
    'pvl-datatable': DataTable,
    'pvl-column': Column,
    'pvl-columngroup': ColumnGroup,
    'pvl-toast': Toast
};

Vue.use(ToastService);

/*
 * Регистрирует компоненты как глобальные с pvl-тегами.
 * */
export function registerGlobalTags() {
    for (var tag in pTagsInternal)
        Vue.component(tag, pTagsInternal[tag]);
}

/*
 * Позволяет использовать компоненты без глобальной регистрации с pvl-тегами.
 * */
export const pTags = pTagsInternal;
