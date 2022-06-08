import PrimeVue from 'primevue/config';
Vue.use(PrimeVue);

import Button from 'primevue/button';
import Menu from 'primevue/menu';
import MenuBar from 'primevue/menubar';
import { FilterMatchMode, FilterOperator } from 'primevue/api';

export const MainHeader = {
    Declaration: {
        components: {
            'pv-button': Button,
            'pv-menu': Menu,
            'pv-menubar': MenuBar
        },
        data: {
            user: {
                logged: false,
                idImage: null,
                access: {
                    controlPanel: false
                }
            },
            ui: {
                urls: {
                    userSigninUrl: '',
                    userSignoutUrl: '',
                    userProfileUrl: '',
                    userImageUrlTemplate: '',
                    controlPanelUrl: ''
                }
            }
        },
        methods: {
            userUpdate: function (val) {
                this.user.logged = val && val.isGuest ? false : true;
                this.user.idImage = val && val.idImage ? Number(val.idImage) : null;
                this.user.access.controlPanel = val && val.access && val.access.controlPanel ? Boolean(val.access.controlPanel) : false;
            },
            onRoute: function (nameUrl) {
                if (!this.ui.urls[nameUrl]) {
                    alert('Некорректный адрес перенаправления!');
                    return;
                }
                window.location = this.ui.urls[nameUrl];
            },
            OnToggleUserMenu: function (event) {
                this.$refs.userMenu.toggle(event);
            }
        },
        computed: {
            userImageUrl: function () {
                return this.user.idImage ? this.ui.urls.userImageUrlTemplate.replace('1234567890', this.user.idImage) : "/data/img/files/user_noimage.jpg";
            },
            userMenuItems: function () {
                var items = new Array();
                if (this.user.logged) {
                    items[items.length] = { label: 'Личный кабинет', icon: 'pi pi-user', url: this.ui.urls.userProfileUrl };
                    if (this.user.access.controlPanel) items[items.length] = { label: 'Панель управления', icon: 'pi pi-cog', url: this.ui.urls.controlPanelUrl, target: '_blank' };
                    items[items.length] = { label: 'Выход', icon: 'pi pi-power-off', url: this.ui.urls.userSignoutUrl };
                }
                return items;
            }
        },
        mounted: function () {
            this.ui.urls.userSigninUrl = this.$el.getAttribute("data-user-signin-url");
            this.ui.urls.userSignoutUrl = this.$el.getAttribute("data-user-signout-url");
            this.ui.urls.userProfileUrl = this.$el.getAttribute("data-user-profile-url");
            this.ui.urls.userImageUrlTemplate = this.$el.getAttribute("data-user-image-template-url");
            this.ui.urls.controlPanelUrl = this.$el.getAttribute("data-controlpanel-url");
        }
    },
    Init: function () {
        MainHeader.Instance = new Vue(Object.assign(MainHeader.Declaration, {
            el: "#js-main-header"
        }));
    },
    Instance: null
};

class PrimeVueDataTableFieldFilterConstraint {
    constructor(source) {
        this.MatchType = "contains";
        switch (source.matchMode) {
            case FilterMatchMode.CONTAINS:
                this.MatchType = 2;
                break;

            case FilterMatchMode.STARTS_WITH:
                this.MatchType = 1;
                break;

            case FilterMatchMode.EQUALS:
                this.MatchType = 3;
                break;

            default:
                throw "Данное условие не поддерживается.";
        }

        this.Value = source.value;
    }
}

class PrimeVueDataTableFieldFilter {
    constructor(field, source) {
        this.FieldName = field;
        this.Operator = 1;
        this.Constraints = new Array();

        if (source) {
            if (source.operator) {
                switch (source.operator) {
                    case FilterOperator.AND:
                        this.Operator = 1;
                        break;

                    case FilterOperator.OR:
                        this.Operator = 2;
                        break;

                    default:
                        throw "Данное условие не поддерживается.";
                }
                if (source.constraints) this.Constraints = source.constraints.filter(x => x.value).map(x => new PrimeVueDataTableFieldFilterConstraint(x));
            }
            else if (source.value) {
                this.Constraints.push(new PrimeVueDataTableFieldFilterConstraint(source));
            }
        }
    }
}

export class PrimeVueDataTableSourceRequest {
    constructor(fieldNameMapper) {
        this.FirstRow = 0;
        this.RowsLimit = 0;
        this.SortByFieldName = null;
        this.SortByAcsending = true;
        this.FilterFields = [];

        this.fieldNameMapper = typeof fieldNameMapper === 'function' ? fieldNameMapper : (val) => val;
    }

    ApplyPagination(source) {
        this.FirstRow = source && source.first ? Number(source.first) : 0;
        this.RowsLimit = source && source.rows ? Number(source.rows) : 0;
    }

    ApplySort(source) {
        this.SortByFieldName = source && source.sortField ? this.fieldNameMapper(String(source.sortField)) : null;
        this.SortByAcsending = !source ? true : source.sortOrder === 1 ? true : false;
    }

    ApplyFilter(source) {
        let filterFields = [];
        for (var field in source) {
            if (source[field]) {
                filterFields.push(new PrimeVueDataTableFieldFilter(this.fieldNameMapper(field), source[field]));
            }
        }
        this.FilterFields = filterFields.filter(x => x.Constraints.length > 0);
    }
}
