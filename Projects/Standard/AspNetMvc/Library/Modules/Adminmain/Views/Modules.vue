<template>
    <div>
        <pvl-tabview>
            <pvl-tabpanel header="Список модулей">
                <pvl-datatable :value="dataList" :row-class="rowClass"
                               sort-field="caption" sort-order="1"
                               :filters="filters"
                               style="width:1200px;">
                    <template #header>
                        <div style='height:20px'>Всего модулей: {{ dataList ? dataList.length : 0 }}</div>
                    </template>
                    <pvl-column field="id" header="№" sortable="true" header-style="width:50px;" filter-match-mode="contains">
                        <template #filter>
                            <pvl-inputtext type="text" v-model="filters['id']" class="p-column-filter"></pvl-inputtext>
                        </template>
                    </pvl-column>
                    <pvl-column field="caption" header="Название" sortable="true" header-style="width:200px;" filter-match-mode="contains">
                        <template #filter>
                            <pvl-inputtext type="text" v-model="filters['caption']" class="p-column-filter"></pvl-inputtext>
                        </template>
                    </pvl-column>
                    <pvl-column field="type" header="Query-тип модуля" sortable="true" filter-match-mode="contains">
                        <template #filter>
                            <pvl-inputtext type="text" v-model="filters['type']" class="p-column-filter"></pvl-inputtext>
                        </template>
                    </pvl-column>
                    <pvl-column field="uniqueName" header="Уникальное имя модуля" sortable="true" header-style="width:200px;" filter-match-mode="contains">
                        <template #filter>
                            <pvl-inputtext type="text" v-model="filters['uniqueName']" class="p-column-filter"></pvl-inputtext>
                        </template>
                    </pvl-column>
                    <pvl-column field="urlName" header="URL-доступное имя модуля" sortable="true" header-style="width:200px;" filter-match-mode="contains">
                        <template #filter>
                            <pvl-inputtext type="text" v-model="filters['urlName']" class="p-column-filter"></pvl-inputtext>
                        </template>
                    </pvl-column>
                    <pvl-column column-key="Actions" header="Действия" header-style="width:100px;">
                        <template #body="slotProps">
                            <pvl-button label="Настройки" @click.stop="onConfigure(slotProps.data)" v-show="slotProps.data.configAllowed"></pvl-button>
                        </template>
                    </pvl-column>
                </pvl-datatable>
            </pvl-tabpanel>
            <pvl-tabpanel :active.sync="moduleCurrent.tabActive" :disabled="!moduleCurrent.data || moduleCurrent.loading">
                <template slot="header">
                    <span>Настройки модуля</span>
                </template>
                <pvl-progressspinner :class="[{'hidden' : !moduleCurrent.loading}]"></pvl-progressspinner>
                <div id="containerForLoading" :class="[{'hidden' : moduleCurrent.loading}]"></div>
            </pvl-tabpanel>
        </pvl-tabview>
    </div>
</template>
<script type='text/javascript'>
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
        data: function () {
            return {
                filters: {},
                moduleCurrent: {
                    data: null,
                    loading: false,
                    tabActive: false,
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
                component.moduleCurrent.tabActive = true;
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