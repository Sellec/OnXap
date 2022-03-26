<template>
    <div>
        <pv-tabview :activeIndex.sync="moduleCurrent.tabActive">
            <pv-tabpanel header="Список модулей">
                <pv-datatable :value="dataList" :row-class="rowClass"
                               sort-field="caption" sort-order="1"
                               :filters="filters"
                               style="width:1200px;">
                    <template #header>
                        <div style='height:20px'>Всего модулей: {{ dataList ? dataList.length : 0 }}</div>
                    </template>
                    <pv-column field="id" header="№" sortable="true" header-style="width:50px;" filter-match-mode="contains">
                        <template #filter>
                            <pv-inputtext type="text" v-model="filters['id']" class="p-column-filter"></pv-inputtext>
                        </template>
                    </pv-column>
                    <pv-column field="caption" header="Название" sortable="true" header-style="width:200px;" filter-match-mode="contains">
                        <template #filter>
                            <pv-inputtext type="text" v-model="filters['caption']" class="p-column-filter"></pv-inputtext>
                        </template>
                    </pv-column>
                    <pv-column field="type" header="Query-тип модуля" sortable="true" filter-match-mode="contains">
                        <template #filter>
                            <pv-inputtext type="text" v-model="filters['type']" class="p-column-filter"></pv-inputtext>
                        </template>
                    </pv-column>
                    <pv-column field="uniqueName" header="Уникальное имя модуля" sortable="true" header-style="width:200px;" filter-match-mode="contains">
                        <template #filter>
                            <pv-inputtext type="text" v-model="filters['uniqueName']" class="p-column-filter"></pv-inputtext>
                        </template>
                    </pv-column>
                    <pv-column field="urlName" header="URL-доступное имя модуля" sortable="true" header-style="width:200px;" filter-match-mode="contains">
                        <template #filter>
                            <pv-inputtext type="text" v-model="filters['urlName']" class="p-column-filter"></pv-inputtext>
                        </template>
                    </pv-column>
                    <pv-column column-key="Actions" header="Действия" header-style="width:100px;">
                        <template #body="slotProps">
                            <pv-button label="Настройки" @click.stop="onConfigure(slotProps.data)" v-show="slotProps.data.configAllowed"></pv-button>
                        </template>
                    </pv-column>
                </pv-datatable>
            </pv-tabpanel>
            <pv-tabpanel :disabled="!moduleCurrent.data || moduleCurrent.loading">
                <template slot="header">
                    <span>Настройки модуля</span>
                </template>
                <pv-progressspinner v-if="moduleCurrent.loading"></pv-progressspinner>
                <div id="containerForLoading" :class="[{'hidden' : moduleCurrent.loading}]"></div>
            </pv-tabpanel>
        </pv-tabview>
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