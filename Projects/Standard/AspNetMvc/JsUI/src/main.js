import Vue from 'vue';

import accordion from 'primevue/accordion';
import accordiontab from 'primevue/accordiontab';
import autocomplete from 'primevue/autocomplete';
import breadcrumb from 'primevue/breadcrumb';
import button from 'primevue/button';
import calendar from 'primevue/calendar';
import card from 'primevue/card';
import carousel from 'primevue/carousel';
//import chart from 'primevue/chart';
import checkbox from 'primevue/checkbox';
import chips from 'primevue/chips';
import colorpicker from 'primevue/colorpicker';
import column from 'primevue/column';
import columngroup from 'primevue/columngroup';
import contextmenu from 'primevue/contextmenu';
import datatable from 'primevue/datatable';
import dataview from 'primevue/dataview';
import dataviewlayoutoptions from 'primevue/dataviewlayoutoptions';
import deferredcontent from 'primevue/deferredcontent';
import dialog from 'primevue/dialog';
import dropdown from 'primevue/dropdown';
import editor from 'primevue/editor';
import fieldset from 'primevue/fieldset';
import fileupload from 'primevue/fileupload';
//import fullcalendar from 'primevue/fullcalendar';
import inplace from 'primevue/inplace';
import inputmask from 'primevue/inputmask';
import inputswitch from 'primevue/inputswitch';
import inputtext from 'primevue/inputtext';
import listbox from 'primevue/listbox';
import megamenu from 'primevue/megamenu';
import menu from 'primevue/menu';
import menubar from 'primevue/menubar';
import message from 'primevue/message';
import multiselect from 'primevue/multiselect';
import orderlist from 'primevue/orderlist';
import organizationchart from 'primevue/organizationchart';
import overlaypanel from 'primevue/overlaypanel';
import paginator from 'primevue/paginator';
import panel from 'primevue/panel';
import panelmenu from 'primevue/panelmenu';
import password from 'primevue/password';
import picklist from 'primevue/picklist';
import progressbar from 'primevue/progressbar';
import progressspinner from 'primevue/progressspinner';
import radiobutton from 'primevue/radiobutton';
import rating from 'primevue/rating';
import row from 'primevue/row';
import selectbutton from 'primevue/selectbutton';
import sidebar from 'primevue/sidebar';
import slider from 'primevue/slider';
import spinner from 'primevue/spinner';
import splitbutton from 'primevue/splitbutton';
import steps from 'primevue/steps';
import tabmenu from 'primevue/tabmenu';
import tabpanel from 'primevue/tabpanel';
import tabview from 'primevue/tabview';
import textarea from 'primevue/textarea';
import tieredmenu from 'primevue/tieredmenu';
import toast from 'primevue/toast';
import toastservice from 'primevue/toastservice';
import togglebutton from 'primevue/togglebutton';
import toolbar from 'primevue/toolbar';
import tooltip from 'primevue/tooltip';
import tree from 'primevue/tree';
import treetable from 'primevue/treetable';
import tristatecheckbox from 'primevue/tristatecheckbox';
import validationmessage from 'primevue/validationmessage';

//import 'quill/quill';

import 'primevue/resources/primevue.css';
import 'primeicons/primeicons.css';

var pTagsInternal = {
    'pvl-accordion': accordion,
    'pvl-accordiontab': accordiontab,
    'pvl-autocomplete': autocomplete,
    'pvl-breadcrumb': breadcrumb,
    'pvl-button': button,
    'pvl-calendar': calendar,
    'pvl-card': card,
    'pvl-carousel': carousel,
    //'pvl-chart': chart,
    'pvl-checkbox': checkbox,
    'pvl-chips': chips,
    'pvl-colorpicker': colorpicker,
    'pvl-column': column,
    'pvl-columngroup': columngroup,
    'pvl-contextmenu': contextmenu,
    'pvl-datatable': datatable,
    'pvl-dataview': dataview,
    'pvl-dataviewlayoutoptions': dataviewlayoutoptions,
    'pvl-deferredcontent': deferredcontent,
    'pvl-dialog': dialog,
    'pvl-dropdown': dropdown,
    'pvl-editor': editor,
    'pvl-fieldset': fieldset,
    'pvl-fileupload': fileupload,
    //'pvl-fullcalendar': fullcalendar,
    'pvl-inplace': inplace,
    'pvl-inputmask': inputmask,
    'pvl-inputswitch': inputswitch,
    'pvl-inputtext': inputtext,
    'pvl-listbox': listbox,
    'pvl-megamenu': megamenu,
    'pvl-menu': menu,
    'pvl-menubar': menubar,
    'pvl-message': message,
    'pvl-multiselect': multiselect,
    'pvl-orderlist': orderlist,
    'pvl-organizationchart': organizationchart,
    'pvl-overlaypanel': overlaypanel,
    'pvl-paginator': paginator,
    'pvl-panel': panel,
    'pvl-panelmenu': panelmenu,
    'pvl-password': password,
    'pvl-picklist': picklist,
    'pvl-progressbar': progressbar,
    'pvl-progressspinner': progressspinner,
    'pvl-radiobutton': radiobutton,
    'pvl-rating': rating,
    'pvl-row': row,
    'pvl-selectbutton': selectbutton,
    'pvl-sidebar': sidebar,
    'pvl-slider': slider,
    'pvl-spinner': spinner,
    'pvl-splitbutton': splitbutton,
    'pvl-steps': steps,
    'pvl-tabmenu': tabmenu,
    'pvl-tabpanel': tabpanel,
    'pvl-tabview': tabview,
    'pvl-textarea': textarea,
    'pvl-tieredmenu': tieredmenu,
    'pvl-toast': toast,
    'pvl-toastservice': toastservice,
    'pvl-togglebutton': togglebutton,
    'pvl-toolbar': toolbar,
    'pvl-tooltip': tooltip,
    'pvl-tree': tree,
    'pvl-treetable': treetable,
    'pvl-tristatecheckbox': tristatecheckbox,
    'pvl-validationmessage': validationmessage
};

/*
 * Регистрирует компоненты как глобальные с pvl-тегами.
 * */
export function registerGlobalTags() {
    Vue.use(pTagsInternal['pvl-toastservice']);

    for (var tag in pTagsInternal)
        Vue.component(tag, pTagsInternal[tag]);
}

/*
 * Позволяет использовать компоненты без глобальной регистрации с pvl-тегами.
 * */
export const pTags = pTagsInternal;
