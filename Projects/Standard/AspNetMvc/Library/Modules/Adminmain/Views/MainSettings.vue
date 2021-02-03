<style>
    div.block-setting {
        margin-bottom: 5px;
        padding: 5px 0px 10px 0px;
    }
</style>
<script type='text/javascript'>
    import Quill from 'quill';
    import Button from 'primevue/button';
    import Dropdown from 'primevue/dropdown';
    import Editor from 'primevue/editor';
    import InputText from 'primevue/inputtext';

    class MainSettingsSave {
        constructor(source) {
            this.AppCoreConfiguration = {
                RoleGuest: source.App.RoleGuest,
                RoleUser: source.App.RoleUser,
                ApplicationTimezoneId: source.App.ApplicationTimezoneId
            };
            this.WebCoreConfiguration = {
                SiteFullName: source.Web.SiteFullName,
                IdModuleDefault: source.Web.IdModuleDefault,
                ContactEmail: source.Web.ContactEmail,
                ReturnEmail: source.Web.ReturnEmail,
                DeveloperEmail: source.Web.DeveloperEmail,
                CriticalMessagesEmail: source.Web.CriticalMessagesEmail,
                register_mode: source.Web.register_mode,
                userAuthorizeAllowed: source.Web.userAuthorizeAllowed,
                site_reginfo: source.Web.site_reginfo,
                site_loginfo: source.Web.site_loginfo,
                help_info: source.Web.help_info,
                site_descr: source.Web.site_descr,
                site_keys: source.Web.site_keys
            };
        }
    }

    export class ViewModel {
        constructor(source, timezones, mainSettingsSaveUrl) {
            this.Model = {
                App: {
                    RoleGuest: Number(source.AppCoreConfiguration.RoleGuest),
                    RoleUser: Number(source.AppCoreConfiguration.RoleUser),
                    ApplicationTimezoneId: String(source.AppCoreConfiguration.ApplicationTimezoneId)
                },
                Web: {
                    SiteFullName: String(source.WebCoreConfiguration.SiteFullName),
                    IdModuleDefault: Number(source.WebCoreConfiguration.IdModuleDefault),
                    ContactEmail: String(source.WebCoreConfiguration.ContactEmail),
                    ReturnEmail: String(source.WebCoreConfiguration.ReturnEmail),
                    DeveloperEmail: String(source.WebCoreConfiguration.DeveloperEmail),
                    CriticalMessagesEmail: String(source.WebCoreConfiguration.CriticalMessagesEmail),
                    register_mode: Number(source.WebCoreConfiguration.register_mode),
                    userAuthorizeAllowed: Number(source.WebCoreConfiguration.userAuthorizeAllowed),
                    site_reginfo: String(source.WebCoreConfiguration.site_reginfo),
                    site_loginfo: String(source.WebCoreConfiguration.site_loginfo),
                    help_info: String(source.WebCoreConfiguration.help_info),
                    site_descr: String(source.WebCoreConfiguration.site_descr),
                    site_keys: String(source.WebCoreConfiguration.site_keys)
                }
            };

            var modules = source.Modules;
            this.Modules = modules ? modules.map(x => ({ Id: x.Id, Caption: x.Caption })) : new Array();

            //var timezones = @TimeZoneInfo.GetSystemTimeZones().jsobject();
            this.Timezones = timezones ? timezones.map(x => ({ Id: x.Id, Caption: x.DisplayName })) : new Array();

            var roles = source.Roles;
            this.Roles = roles ? roles.map(x => ({ Id: x.IdRole, Name: x.NameRole })) : new Array();

            this.urls = {
                mainSettingsSave: String(mainSettingsSaveUrl)
            }
        }
    }

    export default {
        components: {
            'pv-button': Button,
            'pv-dropdown': Dropdown,
            'pv-editor': Editor,
            'pv-inputtext': InputText,
        },
        data: {
            isSaving: false,
            lazyRequestIdLatest: null,
            viewModel: {
                type: ViewModel,
                required: true
            }
        },
        methods: {
            OnSave: function () {
                var component = this;
                component.isSaving = true;
                component.lazyRequestIdLatest = $.requestJSON(this.viewModel.urls.mainSettingsSave, new MainSettingsSave(this.viewModel.Model), function (result, message, data, requestId) {
                    if (component.lazyRequestIdLatest != requestId) return;
                    component.isSaving = false;
                    if (message.length > 0) component.$toast.add({ severity: result == JsonResult.OK ? 'success' : 'error', summary: message, life: result == JsonResult.OK ? 3000 : null });
                });
            }
        }
    };
</script>