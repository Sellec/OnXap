﻿<style>
    .js-journal-events__criticalerror { background-color: #f44141 !important }
    .js-journal-events__error { background-color: #f48341 !important }
    .js-journal-events__warning { background-color: #f4c741 !important }
    .js-journal-events__info { }
    #js-events-list__table td { vertical-align: top; }
</style>

<template>
    <div>
        <DataTable :value="dataList" :paginator="true" :rows="paginatorRows" :row-class="rowClass"
                       :lazy="true" :total-records="dataCountAll" :loading="isLoading"
                       @page="onPage($event)" @sort="onSort($event)" @filter="onFilter($event)"
                       :filters.sync="filters"
                      filterDisplay="row"
                       sort-field="dateEvent" sort-order="-1">
            <template #header>
                <div class="content-valign-center">
                    <span>{{ viewModel.nameJournal }}</span>
                    <span v-if="viewModel.idJournal != -1" style="margin-left:5px;">/</span> <Button label="Очистить журнал" v-if="viewModel.idJournal != -1" :icon="['pi', 'pi-times', {'pi-spinner' : isDeleting}]" @click="onClear"></Button>
                </div>
            </template>
            <Column field="eventType" header="Тип события" sortable="true" resizable="false" header-style="width:150px;">
                <template #filter="{filterModel,filterCallback}">
                    <Dropdown v-model="filterModel.value" @change="filterCallback()" :options="eventTypes" option-label="caption" option-value="id" class="p-column-filter"></Dropdown>
                </template>
                <template #body="slotProps">
                    {{ findEventTypeCaption(slotProps.data.eventType) }}
                </template>
            </Column>
            <Column field="eventCode" header="Код события" sortable="true" resizable="false" header-style="width:90px;">
                <template #filter="{filterModel,filterCallback}">
                    <InputText type="text" v-model="filterModel.value" @keyup="filterCallback()" @change="filterCallback()" class="p-column-filter" style="width:90px;" />
                </template>
            </Column>
            <Column field="dateEvent" header="Дата" sortable="true" resizable="false" header-style="width:120px;">
                <template #body="slotProps">
                    {{ slotProps.data.dateEvent ? slotProps.data.dateEvent.format("YYYY-MM-DD HH:mm:ss") : null }}
                </template>
            </Column>

            <Column v-on="viewModel.idJournal != -1" field="journalName" header="Журнал" sortable="true" header-style="width:200px;" body-style="white-space: wrap;">
                <template #filter="{filterModel,filterCallback}">
                    <InputText type="text" v-model="filterModel.value" @keyup="filterCallback()" @change="filterCallback()" class="p-column-filter" />
                </template>
            </Column>

            <Column field="eventInfo" header="Информация" sortable="true" header-style="width:300px;" body-style="white-space: pre-wrap;">
                <template #filter="{filterModel,filterCallback}">
                    <InputText type="text" v-model="filterModel.value" @keyup="filterCallback()" @change="filterCallback()" class="p-column-filter" />
                </template>
            </Column>
            <Column field="eventInfoFull" header="Информация с детализацией" sortable="true" body-style="white-space: pre-wrap;" header-style="width:500px;">
                <template #filter="{filterModel,filterCallback}">
                    <InputText type="text" v-model="filterModel.value" @keyup="filterCallback()" @change="filterCallback()" class="p-column-filter" />
                </template>
                <template #body="slotProps">{{ slotProps.data.eventInfoFull }}</template>
            </Column>
        </DataTable>
    </div>
</template>
<script type='text/javascript'>
    import { EventType } from '../ViewModels/JournalEventTypes';
    import Button from 'primevue/button';
    import Column from 'primevue/column';
    import DataTable from 'primevue/datatable';
    import Dropdown from 'primevue/dropdown';
    import InputText from 'primevue/inputtext';
    import { FilterMatchMode, FilterOperator } from 'primevue/api';

    export class ViewModel {
        constructor() {
            this.idJournal = 0;
            this.nameJournal = "";
            this.journalDataCountAll = 0;
            this.urls = {
                journalDetailsList: "",
                journalClear: ""
            };
        }
    }

    export class ViewModelRow {
        constructor() {
            this.idJournalData = 0;
            this.journalName = "";
            this.eventType = 0;
            this.dateEvent = new moment();
            this.eventCode = 0;
            this.eventInfo = "";
            this.eventInfoDetailed = "";
            this.exceptionDetailed = "";
        }

        get eventInfoFull() {
            return ((this.eventInfoDetailed ? this.eventInfoDetailed + '\n' : '') + (this.exceptionDetailed ? this.exceptionDetailed + '\n' : '')).trimRight();
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
            'Button': Button,
            'Column': Column,
            'DataTable': DataTable,
            'Dropdown': Dropdown,
            'InputText': InputText
        },
        data: function () {
            return {
                dataList: null,
                filters: {
                    'eventType': { value: null, matchMode: FilterMatchMode.CONTAINS },
                    'eventCode': { value: null, matchMode: FilterMatchMode.CONTAINS },
                    'journalName': { value: null, matchMode: FilterMatchMode.CONTAINS },
                    'eventInfo': { value: null, matchMode: FilterMatchMode.CONTAINS },
                    'eventInfoFull': { value: null, matchMode: FilterMatchMode.CONTAINS }
                },
                eventTypes: [
                    { id: EventType.CriticalError, caption: 'Критическая ошибка' },
                    { id: EventType.Error, caption: 'Ошибка' },
                    { id: EventType.Warning, caption: 'Предупреждение' },
                    { id: EventType.Info, caption: 'Событие' }
                ],
                isLoading: false,
                isDeleting: false,
                paginatorRows: 50,
                requestId: null,
                requestOptions: new StandardUI.PrimeVueDataTableSourceRequest(x => {
                    switch (x) {
                        case 'eventType':
                            return 'EventType';//@Html.NameOf(x => x.JournalData.First().EventType)';

                        case 'dateEvent':
                            return 'DateEvent';//@Html.NameOf(x => x.JournalData.First().DateEvent)';

                        case 'eventCode':
                            return 'EventCode';//@Html.NameOf(x => x.JournalData.First().EventCode)';

                        case 'eventInfo':
                            return 'EventInfo';//@Html.NameOf(x => x.JournalData.First().EventInfo)';

                        case 'eventInfoDetailed':
                            return 'EventInfoDetailed';//@Html.NameOf(x => x.JournalData.First().EventInfoDetailed)';

                        case 'exceptionDetailed':
                            return 'ExceptionDetailed';//@Html.NameOf(x => x.JournalData.First().ExceptionDetailed)';

                        case 'eventInfoFull':
                            return 'EventInfoFull';

                        case 'journalName':
                            return 'JournalName';

                        default:
                            return x;
                    }
                })
            };
        },
        computed:
        {
            dataCountAll: function () {
                return this.viewModel.journalDataCountAll;
            }
        },
        methods:
        {
            rowClass: function (row) {
                if (row.eventType == EventType.CriticalError) return "js-journal-events__criticalerror";
                if (row.eventType == EventType.Error) return "js-journal-events__error";
                if (row.eventType == EventType.Warning) return "js-journal-events__warning";
                if (row.eventType == EventType.Info) return "js-journal-events__info";
                return "";
            },
            onPage: function (event) {
                this.requestOptions.ApplyPagination(event);
                this.onLazy();
            },
            onSort: function (event) {
                this.requestOptions.ApplySort(event);
                this.onLazy();
            },
            onFilter: function (event) {
                //this.requestOptions.ApplySort(event);
                //this.onLazy();
            },
            onLazy: function () {
                var component = this;
                component.isLoading = true;
                component.requestId = $.requestJSON(this.viewModel.urls.journalDetailsList, this.requestOptions, function (result, message, data, requestId) {
                    if (component.requestId != requestId) return;
                    component.isLoading = false;
                    if (message.length > 0) component.$toast.add({ severity: result == JsonResult.OK ? 'success' : 'error', summary: message, life: result == JsonResult.OK ? 3000 : null });
                    if (result == JsonResult.OK) {
                        component.viewModel.journalDataCountAll = Number(data.JournalDataCountAll);
                        if (data.JournalData) {
                            component.dataList = data.JournalData.map(function (source) {
                                return Object.assign(new ViewModelRow(), {
                                    idJournalData: Number(source.IdJournalData),
                                    journalName: String(source.JournalInfo.Name),
                                    eventType: Number(source.EventType),
                                    dateEvent: new moment(source.DateEvent),
                                    eventCode: Number(source.EventCode),
                                    eventInfo: String(source.EventInfo),
                                    eventInfoDetailed: source.EventInfoDetailed ? String(source.EventInfoDetailed) : null,
                                    exceptionDetailed: source.ExceptionDetailed ? String(source.ExceptionDetailed) : null
                                });
                            })
                        }
                        else {
                            component.dataList = null;
                        }
                    }
                });
            },
            onClear: function () {
                var component = this;
                component.isDeleting = true;
                $.requestJSON(this.viewModel.urls.journalClear, null, function (result, message) {
                    component.isDeleting = false;
                    if (message.length > 0) component.$toast.add({ severity: result == JsonResult.OK ? 'success' : 'error', summary: message, life: result == JsonResult.OK ? 3000 : null });
                    if (result == JsonResult.OK) component.onLazy();
                });
            },
            findEventTypeCaption: function (eventType) {
                var eventTypeFound = this.eventTypes.filter(x => x.id == eventType);
                return eventTypeFound.length == 1 ? eventTypeFound[0].caption : '!!!Неизвестный тип события';
            }
        },
        watch: {
            'filters': {
                handler: function (v1, v2) {
                    this.requestOptions.ApplyFilter(this.filters);
                    this.onLazy();
                },
                deep: true
            }
        },
        mounted() {
            this.$toast.removeAllGroups();

            this.requestOptions.ApplyPagination({ first: 0, rows: this.paginatorRows });
            this.requestOptions.ApplySort({ sortField: this.sortField, sortOrder: this.sortOrder });
            this.onLazy();
        }
    };
</script>