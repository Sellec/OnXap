import PrimeVue from 'primevue/config';
Vue.use(PrimeVue);

import * as AdminUsersManageSource from '../Design/admin/AdminUsersManage.vue';

export const AdminUsersManage = {
    ViewModel: AdminUsersManageSource.ViewModel,
    Init: function (model) {
        new Vue({
            el: "#js-users-list__table",
            components: { 'component': AdminUsersManageSource.default },
            data: { r: Object.assign(new AdminUsersManageSource.ViewModel(), model) },
            template: '<component :view-model="r"></component>'
        });
    }
};
