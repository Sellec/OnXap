<style>
    .task-disabled {
        background-color: gray !important
    }

    .task-actions .p-component + .p-component:before {
        margin: 0 5px 5px 0;
    }
</style>
<template>
    <div>
        <pvl-tabview>
            <pvl-tabpanel header="Список задач">
                <pvl-datatable :value="taskList" :row-class="rowClass"
                               sort-field="name" sort-order="1"
                               :filters="filters"
                               style="width:1000px;">
                    <template #header>
                        <div style='height:20px'>Всего задач: {{ taskList ? taskList.length : 0 }}</div>
                    </template>
                    <pvl-column field="name" header="Название задачи" sortable="true" filter-match-mode="contains">
                        <template #filter>
                            <pvl-inputtext type="text" v-model="filters['name']" class="p-column-filter"></pvl-inputtext>
                        </template>
                    </pvl-column>
                    <pvl-column field="isEnabled" header="Состояние" sortable="true" rezisable="false" header-style="width:150px;" filter-match-mode="equals">
                        <template #filter>
                            <pvl-tristatecheckbox v-model="filters['isEnabled']" class="p-column-filter"></pvl-tristatecheckbox>
                        </template>
                        <template #body="slotProps">
                            {{ slotProps.data.isEnabled ? 'Включена' : 'Выключена' }}
                        </template>
                    </pvl-column>
                    <pvl-column field="isConfirmed" header="Подтверждена" sortable="true" rezisable="false" header-style="width:150px;" filter-match-mode="equals">
                        <template #filter>
                            <pvl-tristatecheckbox v-model="filters['isConfirmed']" class="p-column-filter"></pvl-tristatecheckbox>
                        </template>
                        <template #body="slotProps">
                            {{ slotProps.data.isConfirmed ? 'Подтверждена' : 'Не подтверждена' }}
                        </template>
                    </pvl-column>
                    <pvl-column column-key="Actions" header="Действия" header-style="width:150px;" header-class="task-actions">
                        <template #body="slotProps">
                            <pvl-button label="Подробно" @click.stop="onShow(slotProps.data)" style="margin:0 5px 5px 0;"></pvl-button>
                            <pvl-splitbutton label="Выполнить" icon="pi pi-caret-right" :model="taskScheduleRunOptions" @click.stop="onExecute(slotProps.data, true)"></pvl-splitbutton>
                        </template>
                    </pvl-column>
                </pvl-datatable>
            </pvl-tabpanel>
            <pvl-tabpanel :active.sync="taskDescriptionCurrent.tabActive" :hidden="!taskDescriptionCurrent.data">
                <template slot="header">
                    <span>Подробности задачи</span>
                </template>
                <div v-if="taskDescriptionCurrent.data">
                    <div>
                        <h4>Название задачи</h4>
                        <pvl-inputtext type="text" v-model="taskDescriptionCurrent.data.name" style="width:600px;" disabled></pvl-inputtext>
                    </div>
                    <div>
                        <h4>Описание задачи</h4>
                        <pvl-textarea v-model="taskDescriptionCurrent.data.description" style="width:600px;" :autoResize="true" rows="5" cols="30" disabled></pvl-textarea>
                    </div>
                    <div>
                        <h4>Состояние (включена или выключена)</h4>
                        <pvl-checkbox :binary="true" v-model="taskDescriptionCurrent.data.isEnabled" :disabled="taskDescriptionCurrent.isSaving || !taskDescriptionCurrent.data.isConfirmed || !taskDescriptionCurrent.data.allowDisabling"></pvl-checkbox>
                    </div>
                    <div>
                        <h4>Неизменяемые правила запуска (созданные при регистрации задачи):</h4>
                        <pvl-datatable :value="taskDescriptionCurrent.data.scheduleList" :row-class="rowClassSchedule"
                                       style="width:1000px;">
                            <pvl-column field="type" header="Тип правила" header-style="width:250px;">
                                <template #body="slotProps">
                                    {{ slotProps.data.type == 1 ? "Cron" : "Фиксированные дата и время" }}
                                </template>
                            </pvl-column>
                            <pvl-column header="Правило">
                                <template #body="slotProps">
                                    <div class="schedule-designer-cron-readonly" :data-cronvalue="slotProps.data.cron" v-show="slotProps.data.type == 1">
                                        { {{slotProps.data.cron}} }
                                    </div>
                                    <div class="schedule-designer-datetimefixed" v-if="slotProps.data.type == 2">
                                        <pvl-input :disabled="true" v-model="slotProps.data.dateTimeFixed" style="width:100%" />
                                    </div>
                                </template>
                            </pvl-column>
                        </pvl-datatable>
                    </div>
                    <div>
                        <h4>Изменяемые правила запуска (созданные вручную): / <pvl-button icon="pi pi-plus" label="Добавить правило" @click="onNewSchedule" :disabled="!c_allowManualSchedule"></pvl-button></h4>
                        <pvl-datatable :value="taskDescriptionCurrent.data.manualScheduleList" :row-class="rowClassSchedule"
                                       style="width:1000px;">
                            <pvl-column field="type" header="Тип правила" header-style="width:250px;">
                                <template #body="slotProps">
                                    <pvl-dropdown v-model="slotProps.data.type" :options="scheduleTypeList" option-label="label" option-value="value" :disabled="!c_allowManualSchedule"></pvl-dropdown>
                                </template>
                            </pvl-column>
                            <pvl-column field="isEnabled" header="Состояние" header-style="width:150px;">
                                <template #body="slotProps">
                                    <pvl-checkbox v-model="slotProps.data.isEnabled" :binary="true" :disabled="!c_allowManualSchedule"></pvl-checkbox>
                                </template>
                            </pvl-column>
                            <pvl-column header="Правило">
                                <template #body="slotProps">
                                    <div class="schedule-designer-cron" :data-cronvalue="slotProps.data.cron" v-show="slotProps.data.type == 1">
                                        { {{slotProps.data.cron}} } <input type="hidden" v-model.lazy="slotProps.data.cron" />
                                    </div>
                                    <div class="schedule-designer-datetimefixed" v-if="slotProps.data.type == 2">
                                        { {{slotProps.data.dateTimeFixed}} }<br />
                                        <pvl-input :disabled="true" v-model="slotProps.data.dateTimeFixed" style="width:100%" v-if="!c_allowManualSchedule"></pvl-input>
                                        <pvl-calendar v-model="slotProps.data.dateTimeFixed" :show-seconds="true" :show-time="true" style="width:100%" v-if="c_allowManualSchedule"></pvl-calendar><br />
                                    </div>
                                </template>
                            </pvl-column>
                            <pvl-column column-key="Actions" header="Действия" header-style="width:150px;">
                                <template #body="slotProps">
                                    <pvl-button icon="pi pi-trash" label="Удалить" @click.stop="onScheduleRemove(slotProps.data)" :disabled="!c_allowManualSchedule"></pvl-button>
                                </template>
                            </pvl-column>
                        </pvl-datatable>
                    </div>
                    <div>
                        <br />
                        <pvl-button label="Сохранить изменения" :icon="['pi', 'pi-save', {'pi-spin' : taskDescriptionCurrent.isSaving}]" v-if="taskDescriptionCurrent.data.isConfirmed && (taskDescriptionCurrent.data.allowDisabling || taskDescriptionCurrent.data.allowManualSchedule)" @click="onSaveChanges"></pvl-button>
                        <pvl-button label="Задача не подтверждена - изменения запрещены" icon="pi pi-times" v-if="!taskDescriptionCurrent.data.isConfirmed" :disabled="true"></pvl-button>
                        <pvl-button label="Задача подтверждена, но изменения запрещены в настройках задачи" icon="pi pi-times" v-if="taskDescriptionCurrent.data.isConfirmed && !taskDescriptionCurrent.data.allowDisabling && !taskDescriptionCurrent.data.allowManualSchedule" :disabled="true"></pvl-button>
                    </div>
                </div>
            </pvl-tabpanel>
        </pvl-tabview>
    </div>
</template>
<script type='text/javascript'>
    import { TaskOptions } from '../ViewModels/TaskSchedulingTaskOptions';

    class TaskScheduleSave {
        constructor(source) {
            this.Type = source.type;
            this.Cron = source.cron;
            this.DateTimeFixed = source.dateTimeFixed ? moment.utc(source.dateTimeFixed, "YYYY-MM-DD HH:mm").toDate() : null;
            this.IsEnabled = source.isEnabled;
        }
    }

    class TaskSchedule {
        constructor(source) {
            this.type = Number(source.Type);
            this.cron = String(source.Cron);
            this.dateTimeFixed = source.DateTimeFixed ? moment.utc(source.DateTimeFixed).toDate() : null;
            this.isEnabled = Boolean(source.IsEnabled);
        }
    }

    class TaskDescription {
        id = 0;
        name = "";
        description = "";
        isEnabled = "";
        isConfirmed = false;
        allowDisabling = false;
        allowManualSchedule = false;
        scheduleList = new Array();
        manualScheduleList = new Array();

        constructor(source) {
            if (source) this.updateFromSource(source);
        };

        updateFromSource(source) {
            this.id = Number(source.Id);
            this.name = String(source.Name);
            this.description = String(source.Description);
            this.isEnabled = Boolean(source.IsEnabled);
            this.isConfirmed = Boolean(source.IsConfirmed);

            var taskOptions = Number(source.TaskOptions);
            this.allowDisabling = (taskOptions & TaskOptions.AllowDisabling) == TaskOptions.AllowDisabling;
            this.allowManualSchedule = (taskOptions & TaskOptions.AllowManualSchedule) == TaskOptions.AllowManualSchedule;

            var s = source.ScheduleList;
            this.scheduleList = s.map(x => new TaskSchedule(x));
            s = source.ManualScheduleList;
            this.manualScheduleList = s.map(x => new TaskSchedule(x));
        }
    }

    export class ViewModel {
        constructor() {
            this.rows = new Array();
            this.journalDetailsLoadUrl = "";
            this.getExecuteUrl = function (id, waitForCompletion) { };
            this.getSaveUrl = function () { };
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
                scheduleTypeList: [
                    { value: 1, label: 'Cron' },
                    { value: 2, label: 'Фиксированные дата и время' },
                ],
                filters: {
                    'isEnabled': null,
                },
                taskDescriptionCurrent: {
                    data: null,
                    isSaving: false,
                    requestId: null,
                    tabActive: false,
                },
                taskScheduleRunOptions: []
            }
        },
        created: function () {
            this.taskScheduleRunOptions = [
                {
                    label: 'Выполнить и подождать завершения',
                    icon: 'pi pi-caret-right',
                    command: (event) => {
                        console.log(event);
                        //this.onExecute( true);
                    }
                },
            ];
        },
        computed: {
            c_allowManualSchedule: function () {
                return this.taskDescriptionCurrent != null && this.taskDescriptionCurrent.data != null && !this.taskDescriptionCurrent.isSaving && this.taskDescriptionCurrent.data.isConfirmed && this.taskDescriptionCurrent.data.allowManualSchedule;
            },
            taskList: function () {
                var rows = this.viewModel.rows;
                if (!rows) rows = new Array();
                return rows.map(x => new TaskDescription(x));
            }
        },
        methods: {
            rowClass: row => row.isEnabled ? "" : "task-disabled",
            rowClassSchedule: row => "",
            onShow: function (taskDescription) {
                this.taskDescriptionCurrent.data = Object.assign(new TaskDescription(null), taskDescription);
                this.taskDescriptionCurrent.tabActive = true;
                this.$nextTick(function () {
                    $('div.schedule-designer-cron').each(function () {
                        var element = $(this),
                            elementInput = $('input', element);

                        element.
                            removeClass('schedule-designer-cron').
                            bind('cron:change', function (e, value) {
                                elementInput.val(value).change();

                                var event = document.createEvent('Events');
                                event.initEvent('change', true, false);
                                elementInput.get(0).dispatchEvent(event);
                            }).
                            jqCron({
                                lang: 'ru',
                                enabled_minute: true,
                                multiple_mins: true,
                                default_value: $(this).data('cronvalue'),
                                no_reset_button: false
                            });
                    });
                    $('div.schedule-designer-cron-readonly').each(function () {
                        var element = $(this);

                        element.
                            removeClass('schedule-designer-cron-readonly').
                            jqCron({
                                lang: 'ru',
                                enabled_minute: true,
                                multiple_mins: true,
                                default_value: $(this).data('cronvalue'),
                                disabled: true
                            });
                    });
                });
            },
            onExecute: function (taskDescription, waitForCompletion) {
                var component = this;
                $.requestJSON(
                    String(this.viewModel.getExecuteUrl(taskDescription.id, waitForCompletion)),
                    null,
                    function (result, message) {
                        if (message.length > 0) component.$toast.add({ severity: result == JsonResult.OK ? 'success' : 'error', summary: message, life: result == JsonResult.OK ? 3000 : null });
                    }
                );
            },
            onScheduleRemove: function (taskSchedule) {
                this.taskDescriptionCurrent.data.manualScheduleList = this.taskDescriptionCurrent.data.manualScheduleList.filter(x => x !== taskSchedule);
            },
            onNewSchedule: function () {
                this.taskDescriptionCurrent.data.manualScheduleList.push(new TaskSchedule({
                    Id: 0,
                    Type: 1,
                    IsEnabled: true,
                    Cron: '',
                    DateTimeFixed: null
                }));
                this.$nextTick(function () {
                    $('div.schedule-designer-cron').each(function () {
                        var element = $(this),
                            elementInput = $('input', element);

                        element.
                            removeClass('schedule-designer-cron').
                            bind('cron:change', function (e, value) {
                                elementInput.val(value).change();

                                var event = document.createEvent('Events');
                                event.initEvent('change', true, false);
                                elementInput.get(0).dispatchEvent(event);
                            }).
                            jqCron({
                                lang: 'ru',
                                enabled_minute: true,
                                multiple_mins: true,
                                default_value: '',
                                no_reset_button: false
                            });
                    });
                });
            },
            onSaveChanges: function () {
                var component = this;
                component.taskDescriptionCurrent.isSaving = true;
                component.taskDescriptionCurrent.requestId = $.requestJSON(
                    String(this.viewModel.getSaveUrl()),
                    {
                        Id: component.taskDescriptionCurrent.data.id,
                        IsEnabled: component.taskDescriptionCurrent.data.isEnabled,
                        ManualScheduleList: component.taskDescriptionCurrent.data.manualScheduleList.map(x => new TaskScheduleSave(x))
                    },
                    function (result, message, data, requestId) {
                        if (component.taskDescriptionCurrent.requestId != requestId) return;
                        component.taskDescriptionCurrent.isSaving = false;
                        if (message.length > 0) component.$toast.add({ severity: result == JsonResult.OK ? 'success' : 'error', summary: message, life: result == JsonResult.OK ? 3000 : null });
                        if (result == JsonResult.OK) {

                            var model = new TaskDescription(data);
                            var taskInList = component.taskList.find(x => x.id == model.id);
                            if (!taskInList) component.taskList.push(model);
                            else taskInList.updateFromSource(data);
                        }
                    }
                );
            }
        }
    };
</script>