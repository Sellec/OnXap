import * as JournalsSource from '../Views/Journals.vue';
import * as JournalDetailsSource from '../Views/JournalDetails.vue';
import * as MainSettingsSource from '../Views/MainSettings.vue';
import * as ModulesSource from '../Views/Modules.vue';
import * as TaskShedulingSource from '../Views/TaskSheduling.vue';

export const Journals = {
    ViewModel: JournalsSource.ViewModel,
    Init: function (model) {
        new Vue({
            el: "#js-journals",
            components: { 'component': JournalsSource.default },
            data: { r: Object.assign(new JournalsSource.ViewModel(), model) },
            template: '<component :view-model="r"></component>'
        });
    }
};

export const JournalDetails = {
    ViewModel: JournalDetailsSource.ViewModel,
    Init: function (model) {
        new Vue({
            el: "#js-events-list__table",
            components: { 'component': JournalDetailsSource.default },
            data: { r: Object.assign(new JournalDetailsSource.ViewModel(), model) },
            template: '<component :view-model="r"></component>'
        });
    }
};

export const MainSettings = {
    ViewModel: MainSettingsSource.ViewModel,
    Init: function (model) {
        var opts = MainSettingsSource.default;
        opts.el = "#js-core-settings";
        opts.data = Object.assign(opts.data, { viewModel: model });
        console.log(opts);
        new Vue(opts);
    }
};

export const Modules = {
    ViewModel: ModulesSource.ViewModel,
    Init: function (model) {
        new Vue({
            el: "#js-modules",
            components: { 'component': ModulesSource.default },
            data: { r: Object.assign(new ModulesSource.ViewModel(), model) },
            template: '<component :view-model="r"></component>'
        });
    }
};

export const TaskSheduling = {
    ViewModel: TaskShedulingSource.ViewModel,
    Init: function (model) {
        new Vue({
            el: "#js-tasksheduling",
            components: { 'component': TaskShedulingSource.default },
            data: { r: Object.assign(new TaskShedulingSource.ViewModel(), model) },
            template: '<component :view-model="r"></component>'
        });
    }
};


