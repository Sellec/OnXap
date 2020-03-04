PrimeVueLibrary.registerGlobalTags();

var vueMainHeader = new Vue({
    el: "#js-main-header",
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
        userUpdate(val) {
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
                items[items.length] = { label: 'Выход', icon: 'pi pi-power-off', url: this.ui.urls.userProfileUrl };
            }
            return items;
        }
    },
    mounted() {
        this.ui.urls.userSigninUrl = this.$el.getAttribute("data-user-signin-url");
        this.ui.urls.userSignoutUrl = this.$el.getAttribute("data-user-signout-url");
        this.ui.urls.userProfileUrl = this.$el.getAttribute("data-user-profile-url");
        this.ui.urls.userImageUrlTemplate = this.$el.getAttribute("data-user-image-template-url");
        this.ui.urls.controlPanelUrl = this.$el.getAttribute("data-controlpanel-url");
    }
});
