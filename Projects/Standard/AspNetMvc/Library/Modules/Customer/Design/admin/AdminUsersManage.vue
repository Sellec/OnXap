<template>
    <div style="width:1500px;">
        <DataTable :value="DataList" :paginator="true" :rows="paginatorRows"
                :lazy="true" :total-records="DataCountAll" :loading="IsLoading"
                @page="OnPage($event)" @sort="OnSort($event)" 
                :filters.sync="filters"
                filterDisplay="row"
                sort-field="IdUser" sort-order="1">
            <Column field="IdUser" header="#" sortable="true" resizable="false" header-style="width:100px;">
                <template #filter="{filterModel,filterCallback}">
                    <InputText type="text" v-model="filterModel.value" @keyup="filterCallback()" @change="filterCallback()" class="p-column-filter" style="width:70px;" />
                </template>
            </Column>
            <Column field="Requisites" header="Реквизиты" sortable="true" resizable="true" body-style="white-space: pre-wrap;">
                <template #filter="{filterModel,filterCallback}">
                    <InputText type="text" v-model="filterModel.value" @keyup="filterCallback()" @change="filterCallback()" class="p-column-filter" />
                </template>
            </Column>
            <Column field="Superuser" header="Права" sortable="true" resizable="false" header-style="width:130px;">
                <template #body="slotProps">
                    {{ slotProps.data.Superuser > 0 ? 'Суперпользователь' : 'Пользователь' }}
                </template>
            </Column>
            <Column field="State" header="Состояние" sortable="true" resizable="false" header-style="width:250px;">
                <template #body="slotProps">
                    {{ viewModel.userStateList[slotProps.data.State] }}
                </template>
            </Column>
            <Column column-key="Actions" header="Действия" resizable="false" header-style="width:300px;">
                <template #body="slotProps">
                    <a :href="String(viewModel.Urls.userEditCallback(slotProps.data.IdUser))" target='_blank' class='user_link'>Редактировать</a>
                    <span style="margin: 0 5px 0 5px;"> /</span>
                    <a :href="String(viewModel.Urls.userRolesManageCallback(slotProps.data.IdUser))" target='_blank' class='user_link'>Права доступа</a>
                    <br>
                    <a href="#" @click.prevent="OnDeleteUser(slotProps.data)">Удалить</a>
                    <span v-if="viewModel.isSuperuser" style="margin: 0 5px 0 5px;"> /</span>
                    <a v-if="viewModel.isSuperuser" :href="String(viewModel.Urls.userEnterAsCallback(slotProps.data.IdUser))" class='user_link'>Зайти от имени</a>
                </template>
            </Column>
            <Column field="CommentAdmin" header="Комментарий" sortable="true" resizable="true" header-style="width:200px;">
                <template #filter="{filterModel,filterCallback}">
                    <InputText type="text" v-model="filterModel.value" @keyup="filterCallback()" @change="filterCallback()" class="p-column-filter" />
                </template>
            </Column>
        </DataTable>
    </div>
</template>
<script type='text/javascript'>
    import Column from 'primevue/column';
    import DataTable from 'primevue/datatable';
    import InputText from 'primevue/inputtext';
    import { FilterMatchMode, FilterOperator } from 'primevue/api';

    class ViewModelRow {
        IdUser = 0;
        name = "";
        email = "";
        phone = "";
        Superuser = 0;
        State = 0;
        CommentAdmin = "";

        constructor(source) {
            if (source) {
                this.IdUser = Number(source.IdUser);
                this.name = source.name ? String(source.name) : null;
                this.email = source.email ? String(source.email) : null;
                this.phone = source.phone ? String(source.phone) : null;
                this.Superuser = Number(source.Superuser);
                this.State = Number(source.State);
                this.CommentAdmin = source.CommentAdmin ? String(source.CommentAdmin) : null;
            }
        }

        get Requisites() {
            return ((this.name ? this.name + "\n" : '') + (this.email ? this.email + "\n" : '') + (this.phone ? this.phone + "\n" : '')).trimRight();
        }
    }

    export class ViewModel {
        DataList = new Array();
        DataCountAll = 0;
        userStateList = new Array();
        Urls = {
            usersList: "",
            userDeleteCallback: function (idUser) { return ""; },
            userEditCallback: function (idUser) { return ""; },
            userRolesManageCallback: function (idUser) { return ""; },
            userEnterAsCallback: function (idUser) { return ""; }
        };
        isSuperuser = false;

        constructor(source, userStateList, isSuperuser, urlUsersList, urlUserDeleteCallback, urlUserEditCallback, urlUserRolesManageCallback, urlUserEnterAsCallback) {
            if (source) {
                var dataList = source.DataList;
                this.DataList = dataList ? dataList.map(x => new ViewModelRow(x)) : null;
                this.DataCountAll = Number(source.DataCountAll);
            }
            if (userStateList) {
                this.userStateList = userStateList;
            }
            this.isSuperuser = Boolean(isSuperuser);
            this.Urls.usersList = urlUsersList ? String(urlUsersList) : null;
            this.Urls.userDeleteCallback = urlUserDeleteCallback instanceof Function ? urlUserDeleteCallback : function () { return null; };
            this.Urls.userEditCallback = urlUserEditCallback instanceof Function ? urlUserEditCallback : function () { return null; };
            this.Urls.userRolesManageCallback = urlUserRolesManageCallback instanceof Function ? urlUserRolesManageCallback : function () { return null; };
            this.Urls.userEnterAsCallback = urlUserEnterAsCallback instanceof Function ? urlUserEnterAsCallback : function () { return null; };
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
            'Column': Column,
            'DataTable': DataTable,
            'InputText': InputText
        },
        data: function () {
            return {
                filters: {
                    'IdUser': { value: null, matchMode: FilterMatchMode.CONTAINS },
                    'Requisites': { value: null, matchMode: FilterMatchMode.CONTAINS },
                    'CommentAdmin': { value: null, matchMode: FilterMatchMode.CONTAINS }
                },
                IsLoading: false,
                DataCountAll: 0,
                DataList: null,
                LazyRequestOptions: new StandardUI.PrimeVueDataTableSourceRequest(),
                paginatorRows: 50,
                lazyRequestIdLatest: null
            }
        },
        methods:
        {
            OnPage: function (event) {
                this.LazyRequestOptions.ApplyPagination(event);
                this.OnLazy();
            },
            OnSort: function (event) {
                this.LazyRequestOptions.ApplySort(event);
                this.OnLazy();
            },
            OnLazy: function () {
                var component = this;
                component.IsLoading = true;

                component.lazyRequestIdLatest = $.requestJSON(this.viewModel.Urls.usersList, this.LazyRequestOptions, function (result, message, data, requestId) {
                    if (component.lazyRequestIdLatest != requestId) return;
                    component.IsLoading = false;
                    if (message.length > 0) component.$toast.add({ severity: result == JsonResult.OK ? 'success' : 'error', summary: message, life: result == JsonResult.OK ? 3000 : null });
                    if (result == JsonResult.OK) {
                        var data = new ViewModel(data);
                        component.DataCountAll = data.DataCountAll;
                        component.DataList = data.DataList;
                    }
                });
            },
            OnDeleteUser: function (row) {
                var _this = this;
                if (row != null && confirm('Вы действительно хотите удалить пользователя №' + row.IdUser + ' "' + row.email + '"?')) {
                    $.requestJSON(String(this.viewModel.Urls.userDeleteCallback(row.IdUser)), null, function (result, message) {
                        if (result == JsonResult.OK) {
                            var a = new Array();
                            for (var i in _this.DataList) {
                                if (_this.DataList[i].IdUser == row.IdUser) _this.DataCountAll--;
                                else a[a.length] = _this.DataList[i];
                            }
                            _this.DataList = a;
                        }
                        if (message.length > 0) _this.$toast.add({ severity: result == JsonResult.OK ? 'success' : 'error', summary: message, life: result == JsonResult.OK ? 3000 : null });
                    });
                }
            }
        },
        watch: {
            'filters': {
                handler: function (v1, v2) {
                    this.LazyRequestOptions.ApplyFilter(this.filters);
                    this.OnLazy();
                },
                deep: true
            }
        },
        mounted() {
            var component = this;
            component.$toast.removeAllGroups();

            this.LazyRequestOptions.ApplyPagination({ first: 0, rows: this.paginatorRows });
            this.LazyRequestOptions.ApplySort({ sortField: this.sortField, sortOrder: this.sortOrder });
            this.OnLazy();
        }
    };
</script>