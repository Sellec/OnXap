﻿<template>
    <div>
        <TabView :activeIndex.sync="moduleCurrent.tabActive">
            <TabPanel header="Список модулей">
                <DataTable :value="dataList" :row-class="rowClass"
                        sort-field="caption" sort-order="1"
                        :filters.sync="filters"
                        filterDisplay="row"
                        style="width:1200px;">
                    <template #header>
                        <div style='height:20px'>Всего модулей: {{ dataList ? dataList.length : 0 }}</div>
                    </template>
                    <Column field="id" header="№" sortable="true" header-style="width:50px;">
                        <template #filter="{filterModel,filterCallback}">
                            <InputText type="text" v-model="filterModel.value" @keyup="filterCallback()" @change="filterCallback()" class="p-column-filter"></InputText>
                        </template>
                    </Column>
                    <Column field="caption" header="Название" sortable="true" header-style="width:200px;">
                        <template #filter="{filterModel,filterCallback}">
                            <InputText type="text" v-model="filterModel.value" @keyup="filterCallback()" @change="filterCallback()" class="p-column-filter"></InputText>
                        </template>
                    </Column>
                    <Column field="type" header="Query-тип модуля" sortable="true">
                        <template #filter="{filterModel,filterCallback}">
                            <InputText type="text" v-model="filterModel.value" @keyup="filterCallback()" @change="filterCallback()" class="p-column-filter"></InputText>
                        </template>
                    </Column>
                    <Column field="uniqueName" header="Уникальное имя модуля" sortable="true" header-style="width:200px;">
                        <template #filter="{filterModel,filterCallback}">
                            <InputText type="text" v-model="filterModel.value" @keyup="filterCallback()" @change="filterCallback()" class="p-column-filter"></InputText>
                        </template>
                    </Column>
                    <Column field="urlName" header="URL-доступное имя модуля" sortable="true" header-style="width:200px;">
                        <template #filter="{filterModel,filterCallback}">
                            <InputText type="text" v-model="filterModel.value" @keyup="filterCallback()" @change="filterCallback()" class="p-column-filter"></InputText>
                        </template>
                    </Column>
                    <Column column-key="Actions" header="Действия" header-style="width:100px;">
                        <template #body="slotProps">
                            <Button label="Настройки" @click.stop="onConfigure(slotProps.data)" v-show="slotProps.data.configAllowed"></Button>
                        </template>
                    </Column>
                </DataTable>
            </TabPanel>
            <TabPanel :disabled="!moduleCurrent.data || moduleCurrent.loading">
                <template slot="header">
                    <span>Настройки модуля</span>
                </template>
                <ProgressSpinner v-if="moduleCurrent.loading" />
                <div id="containerForLoading" :class="[{'hidden' : moduleCurrent.loading}]"></div>
            </TabPanel>
        </TabView>
    </div>
</template>
<script type='text/javascript'>
    import Button from 'primevue/button';
    import Column from 'primevue/column';
    import DataTable from 'primevue/datatable';
    import InputText from 'primevue/inputtext';
    import ProgressSpinner from 'primevue/progressspinner';
    import TabPanel from 'primevue/tabpanel';
    import TabView from 'primevue/tabview';
    import { FilterMatchMode, FilterOperator } from 'primevue/api';

    export class ViewModel {
        constructor() {
            this.rows = new Array();
            this.moduleConfigUrl = "";
        }
    }

    export class ViewModelRow {
        constructor() {
            this.id = 0;
            this.caption = "";
            this.configAllowed = false;
            this.type = "";
            this.uniqueName = "";
            this.urlName = "";
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
            'InputText': InputText,
            'ProgressSpinner': ProgressSpinner,
            'TabPanel': TabPanel,
            'TabView': TabView
        },
        data: function () {
            return {
                filters: {
                    'id': { value: null, matchMode: FilterMatchMode.CONTAINS },
                    'caption': { value: null, matchMode: FilterMatchMode.CONTAINS },
                    'type': { value: null, matchMode: FilterMatchMode.CONTAINS },
                    'uniqueName': { value: null, matchMode: FilterMatchMode.CONTAINS },
                    'urlName': { value: null, matchMode: FilterMatchMode.CONTAINS }
                },
                moduleCurrent: {
                    data: null,
                    loading: false,
                    tabActive: 0,
                    requestId: null,
                }
            };
        },
        computed: {
            dataList: function () {
                var rows = this.viewModel.rows;
                if (!rows) rows = new Array();
                return rows.map(x => Object.assign(new ViewModelRow(), x));
            }
        },
        methods: {
            onConfigure: function (moduleData) {
                var component = this;
                component.moduleCurrent.loading = true;
                component.moduleCurrent.tabActive = 1;
                component.moduleCurrent.requestId = $("#containerForLoading").requestLoad(
                    this.viewModel.moduleConfigUrl.replace('{0}', moduleData.id),
                    null,
                    function (result, message, data, requestId) {
                        if (component.moduleCurrent.requestId != requestId) return;
                        component.moduleCurrent.loading = false;
                        if (message.length > 0) alert(message);
                        if (result == JsonResult.OK) {
                            component.moduleCurrent.data = moduleData;
                        }
                    }
                );
            }
        }
    };
</script>