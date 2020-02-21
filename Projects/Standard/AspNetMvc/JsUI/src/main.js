//import Vue from 'vue'; // �� ������, ��� Vue.esm ���� �������� � ������.
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
 * ������������ ���������� ��� ���������� � pvl-������.
 * */
export function registerGlobalTags() {
    for (var tag in pTagsInternal)
        Vue.component(tag, pTagsInternal[tag]);
}

/*
 * ��������� ������������ ���������� ��� ���������� ����������� � pvl-������.
 * */
export const pTags = pTagsInternal;
