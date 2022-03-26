<style>
    .js-journal-events__criticalerror { background-color: #f44141 !important }
    .js-journal-events__error { background-color: #f48341 !important }
    .js-journal-events__warning { background-color: #f4c741 !important }
    .js-journal-events__info { }
</style>
<template>
    <div>
        <pv-tabview :activeIndex.sync="journalCurrent.tabActive">
            <pv-tabpanel header="Список журналов">
                <pv-datatable :value="dataList" :row-class="rowClass"
                               sort-field="nameJournal" sort-order="1"
                               :filters="filters"
                               style="width:1000px;">
                    <template #header>
                        <div style='height:20px'>
                            Всего журналов: {{ dataList ? dataList.length : 0 }}
                        </div>
                    </template>
                    <pv-column field="nameJournal" header="Название журнала" sortable="true" filter-match-mode="contains">
                        <template #filter>
                            <pv-inputtext type="text" v-model="filters['nameJournal']" class="p-column-filter"></pv-inputtext>
                            <pv-button label="Показать события всех журналов" @click.stop="onShow({ idJournal : -1 })"></pv-button>
                        </template>
                    </pv-column>
                    <pv-column field="eventsCount" header="Количество событий" sortable="true" header-style="width:100px;" filter-match-mode="contains"></pv-column>
                    <pv-column field="latestEventDate" header="Дата" sortable="true" rezisable="false" header-style="width:150px;">
                        <template #body="slotProps">
                            {{ slotProps.data.latestEventDate ? slotProps.data.latestEventDate.format("YYYY-MM-DD HH:mm:ss") : null }}
                        </template>
                    </pv-column>
                    <pv-column column-key="Actions" header="Действия" header-style="width:150px;">
                        <template #body="slotProps">
                            <pv-button label="Подробности" @click.stop="onShow(slotProps.data)"></pv-button>
                        </template>
                    </pv-column>
                </pv-datatable>
            </pv-tabpanel>
            <pv-tabpanel :disabled="!journalCurrent.data || journalCurrent.loading">
                <template slot="header">
                    <span>Подробности журнала</span>
                </template>
                <pv-progressspinner v-if="journalCurrent.loading"></pv-progressspinner>
                <div id="containerForLoading" :class="[{'hidden' : journalCurrent.loading}]"></div>
            </pv-tabpanel>
        </pv-tabview>
    </div>
</template>
<script type='text/javascript'>
    import { EventType } from '../ViewModels/JournalEventTypes';
    import Button from 'primevue/button';
    import Column from 'primevue/column';
    import DataTable from 'primevue/datatable';
    import InputText from 'primevue/inputtext';
    import ProgressSpinner from 'primevue/progressspinner';
    import TabPanel from 'primevue/tabpanel';
    import TabView from 'primevue/tabview';

    export class ViewModel {
        constructor() {
            this.rows = new Array();
            this.journalDetailsLoadUrl = "";
        }
    }

    export class ViewModelRow {
        constructor() {
            this.idJournal = 0;
            this.nameJournal = "";
            this.latestEventType = 0;
            this.latestEventDate = new moment();
            this.eventsCount = 0;
        }
    }

    export default {
        props: {
            viewModel: {
                type: ViewModel,
                required: true
            }
        },
        components: {
            'pv-button': Button,
            'pv-column': Column,
            'pv-datatable': DataTable,
            'pv-inputtext': InputText,
            'pv-progressspinner': ProgressSpinner,
            'pv-tabpanel': TabPanel,
            'pv-tabview': TabView
        },
        data: function () {
            return {
                filters: {},
                journalCurrent: {
                    data: null,
                    loading: false,
                    tabActive: 0,
                    requestId: null,
                }
            }
        },
        computed: {
            dataList: function () {
                var rows = this.viewModel.rows;
                if (!rows) rows = new Array();
                return rows.map(x => Object.assign(new ViewModelRow(), x));
            }
        },
        methods: {
            rowClass: function (row) {
                if (row.latestEventType == EventType.CriticalError) return "js-journal-events__criticalerror";
                if (row.latestEventType == EventType.Error) return "js-journal-events__error";
                if (row.latestEventType == EventType.Warning) return "js-journal-events__warning";
                if (row.latestEventType == EventType.Info) return "js-journal-events__info";
                return "";
            },
            onShow: function (journalData) {
                var component = this;
                component.journalCurrent.loading = true;
                component.journalCurrent.tabActive = 1;
                component.journalCurrent.requestId = $("#containerForLoading").requestLoad(
                    component.viewModel.journalDetailsLoadUrl + journalData.idJournal,
                    null,
                    function (result, message, data, requestId) {
                        if (component.journalCurrent.requestId != requestId) return;
                        component.journalCurrent.loading = false;
                        if (message.length > 0) alert(message);
                        if (result == JsonResult.OK) {
                            component.journalCurrent.data = journalData;
                        }
                        else {
                            console.log($(this));
                        }
                    }
                );
            }
        }
    };
</script>