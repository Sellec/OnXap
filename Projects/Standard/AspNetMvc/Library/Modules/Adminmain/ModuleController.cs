using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web.Mvc;
using OnUtils;
using OnUtils.Types;

namespace OnXap.Modules.Adminmain
{
    using AdminForModules.Menu;
    using Binding.Routing;
    using Core.Configuration;
    using Core.Db;
    using Core.Modules;
    using Journaling;
    using Modules.CoreModule;
    using OnXap.TaskSheduling;
    using Routing.Db;
    using WebCoreModule;
    using JournalingDB = Journaling.DB;

    /// <summary>
    /// Представляет контроллер для панели управления.
    /// </summary>
    public class ModuleController : ModuleControllerAdmin<Module>
    {
        [MenuAction("Состояние системы", null, ModulesConstants.PermissionManageString)]
        public override ActionResult Index()
        {
            return View("SystemState.cshtml");
        }

        public virtual JsonResult SystemRestart()
        {
            try
            {
                if (Module.CheckPermission(Module.PERM_RESTART) != CheckPermissionVariant.Allowed)
                {
                    RegisterEventWithCode(HttpStatusCode.Forbidden, "Попытка перезагрузки.", "Недостаточно прав");
                    return ReturnJson(false, "Недостаточно прав для перезагрузки системы.");
                }

                RegisterEventWithCode(HttpStatusCode.Accepted, "Попытка перезагрузки.", null);
                System.Web.HttpRuntime.UnloadAppDomain();

                return ReturnJson(true, "Перезагрузка запущена.");
            }
            catch (Exception ex)
            {
                RegisterEventWithCode(HttpStatusCode.InternalServerError, "Неожиданная ошибка при попытке перезагрузки", null, ex);
                return ReturnJson(false, "Неожиднная ошибка во время перезагрузки системы.", ex.Message);
            }
        }

        [MenuAction("Настройки", "info", Module.PERM_CONFIGMAIN)]
        public virtual ActionResult MainSettings()
        {
            var handler = AppCore.Get<ModuleControllerTypesManager>();
            var model = new ViewModels.MainSettings(AppCore.AppConfig, AppCore.WebConfig);

            model.Modules = AppCore.GetModulesManager().GetModules().
                Where(x => handler.GetModuleControllerTypes(x.QueryType) != null).
                Select(x => new ViewModels.MainSettingsModule() { Id = x.IdModule, Caption = x.Caption }).
                ToList();

            using (var db = new DataContext())
            {
                model.Roles = db.Role.Where(x => !x.IsHidden || x.IdRole == AppCore.AppConfig.RoleGuest || x.IdRole == AppCore.AppConfig.RoleUser).ToList();
                model.Roles.Insert(0, new Role() { IdRole = 0, NameRole = "Не выбрано", IsHidden = false });
            }

            return View("MainSettings.tpl", model);
        }

        [ModuleAction("info_save", Module.PERM_CONFIGMAIN)]
        public virtual JsonResult MainSettingsSave(Model.MainSettingsSave model)
        {
            var result = JsonAnswer();
            CoreConfiguration cfgAppOld = null;
            WebCoreConfiguration cfgWebOld = null;

            try
            {
                if (ModelState.IsValid)
                {
                    cfgAppOld = AppCore.AppCoreModule.GetConfigurationManipulator().GetEditable<CoreConfiguration>();
                    cfgWebOld = AppCore.GetModulesManager().GetModule<WebCoreModule>().GetConfigurationManipulator().GetEditable<WebCoreConfiguration>();

                    var cfgApp = AppCore.GetModulesManager().GetModule<CoreModule>().GetConfigurationManipulator().GetEditable<CoreConfiguration>();
                    cfgApp.RoleGuest = model.AppCoreConfiguration.RoleGuest;
                    cfgApp.RoleUser = model.AppCoreConfiguration.RoleUser;
                    cfgApp.ApplicationTimezoneId = model.AppCoreConfiguration.ApplicationTimezoneId;

                    var applyResult = AppCore.GetModulesManager().GetModule<CoreModule>().GetConfigurationManipulator().ApplyConfiguration(cfgApp);
                    switch (applyResult.Item1)
                    {
                        case ApplyConfigurationResult.PermissionDenied:
                            result.Message = "Недостаточно прав для сохранения конфигурации системы.";
                            result.Success = false;
                            break;

                        case ApplyConfigurationResult.Failed:
                            var journalData = AppCore.Get<JournalingManager>().GetJournalData(applyResult.Item2.Value);
                            result.Message = $"Возникла ошибка при сохранении настроек: {(journalData.Message ?? "текст ошибки не найден")}.";
                            result.Success = false;
                            break;

                        case ApplyConfigurationResult.Success:
                            System.Web.Routing.RouteTable.Routes.Where(x => x is RouteWithDefaults).Select(x => x as RouteWithDefaults).ForEach(x => x.UpdateDefaults());

                            result.Message = "Сохранено успешно!";
                            result.Success = true;
                            break;
                    }

                    if (result.Success)
                    {
                        var cfg = AppCore.GetModulesManager().GetModule<WebCoreModule>().GetConfigurationManipulator().GetEditable<WebCoreConfiguration>();
                        cfg.IdModuleDefault = model.WebCoreConfiguration.IdModuleDefault;
                        cfg.DeveloperEmail = model.WebCoreConfiguration.DeveloperEmail;
                        cfg.SiteFullName = model.WebCoreConfiguration.SiteFullName;
                        cfg.ContactEmail = model.WebCoreConfiguration.ContactEmail;
                        cfg.ReturnEmail = model.WebCoreConfiguration.ReturnEmail;
                        cfg.CriticalMessagesEmail = model.WebCoreConfiguration.CriticalMessagesEmail;
                        cfg.register_mode = model.WebCoreConfiguration.register_mode;
                        cfg.site_reginfo = model.WebCoreConfiguration.site_reginfo;
                        cfg.site_loginfo = model.WebCoreConfiguration.site_loginfo;
                        cfg.help_info = model.WebCoreConfiguration.help_info;
                        cfg.site_descr = model.WebCoreConfiguration.site_descr;
                        cfg.site_keys = model.WebCoreConfiguration.site_keys;

                        cfg.userAuthorizeAllowed = model.WebCoreConfiguration.userAuthorizeAllowed;

                        applyResult = AppCore.GetModulesManager().GetModule<WebCoreModule>().GetConfigurationManipulator().ApplyConfiguration(cfg);
                        switch (applyResult.Item1)
                        {
                            case ApplyConfigurationResult.PermissionDenied:
                                result.Message = "Недостаточно прав для сохранения конфигурации системы.";
                                result.Success = false;
                                break;

                            case ApplyConfigurationResult.Failed:
                                var journalData = AppCore.Get<JournalingManager>().GetJournalData(applyResult.Item2.Value);
                                result.Message = $"Возникла ошибка при сохранении настроек: {(journalData.Message ?? "текст ошибки не найден")}.";
                                result.Success = false;
                                break;

                            case ApplyConfigurationResult.Success:
                                System.Web.Routing.RouteTable.Routes.Where(x => x is RouteWithDefaults).Select(x => x as RouteWithDefaults).ForEach(x => x.UpdateDefaults());

                                result.Message = "Сохранено успешно!";
                                result.Success = true;
                                break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            try
            {
                if (!result.Success)
                {
                    if (cfgAppOld != null) AppCore.GetModulesManager().GetModule<CoreModule>().GetConfigurationManipulator().ApplyConfiguration(cfgAppOld);
                    if (cfgWebOld != null) AppCore.GetModulesManager().GetModule<WebCoreModule>().GetConfigurationManipulator().ApplyConfiguration(cfgWebOld);
                }
            }
            catch { }

            return this.ReturnJson(result);
        }

        [MenuAction("Модули", "modules", Module.PERM_MODULES)]
        public ActionResult ModulesList()
        {
            var model = new Model.Modules()
            {
                //Unregistered = (from p in AppCore.GetModulesManager().getModulesCandidates()
                //                orderby p.Key.Info.ModuleCaption
                //                select new Model.ModuleUnregistered(p.Key) { Info = p.Value }).ToList(),
                Registered = (from p in AppCore.GetModulesManager().GetModules().OfType<IModuleCore>()
                              orderby p.Caption
                              select new Model.Module(p)).ToList()
            };

            return View("Modules.cshtml", model);
        }

        [MenuAction("Маршрутизация (ЧПУ)", "routing", Module.PERM_ROUTING)]
        public virtual ActionResult Routing()
        {
            var model = new Model.Routing() { Modules = AppCore.GetModulesManager().GetModules().OfType<IModuleCore>().OrderBy(x => x.Caption).ToList() };

            using (var db = new Modules.Routing.Db.DataContext())
            {
                //test group
                var modulesIdList = model.Modules.Select(x => x.IdModule).ToArray();
                var query = db.Routing
                                .Where(x => modulesIdList.Contains(x.IdModule))
                                .GroupBy(x => new { x.IdModule, x.IdRoutingType })
                                .Select(x => new { x.Key.IdModule, x.Key.IdRoutingType, Count = x.Count() })
                                .GroupBy(x => x.IdRoutingType)
                                .ToDictionary(x => x.Key, x => x.ToDictionary(y => y.IdModule, y => y.Count))
                                ;

                var query2 = (new RoutingType[] { RoutingType.Main, RoutingType.Additional, RoutingType.Old })
                                .ToDictionary(x => x,
                                              x => model.Modules.ToDictionary(y => y.IdModule,
                                                                                   y => query.ContainsKey(x) && query[x].ContainsKey(y.IdModule) ? query[x][y.IdModule] : 0));

                model.RoutesMain = query2[RoutingType.Main];
                model.RoutesAdditional = query2[RoutingType.Additional];
                model.RoutesOld = query2[RoutingType.Old];
            }

            return View("routing.cshtml", model);
        }

        [ModuleAction("routingModule", Module.PERM_ROUTING)]
        public virtual ActionResult RoutingModule(int IdModule)
        {
            if (IdModule <= 0) throw new Exception("Не указан идентификатор модуля.");
            var module = AppCore.GetModulesManager().GetModule(IdModule);

            if (module == null) throw new Exception($"Не получилось найти модуль с указанным идентификатором {IdModule}.");

            var model = new Model.RoutingModule() { Module = (IModuleCore)module };

            var moduleActionAttributeType = typeof(ModuleActionAttribute);
            var moduleActionGetDisplayName = new Func<ActionDescriptor, string>(action =>
            {
                var attr = action.GetCustomAttributes(moduleActionAttributeType, true).OfType<ModuleActionAttribute>().FirstOrDefault();
                if (attr != null)
                {
                    if (!string.IsNullOrEmpty(attr.Caption)) return attr.Caption;
                }

                return action.ActionName;
            });

            var modulesActions = AppCore.GetModulesManager().GetModules().OfType<IModuleCore>().Select(x => new
            {
                Module = x,
                UserDescriptor = x.ControllerUser() != null ? new ReflectedControllerDescriptor(x.ControllerUser()) : null,
                AdminDescriptor = x.ControllerAdmin() != null ? new ReflectedControllerDescriptor(x.ControllerAdmin()) : null,
            }).Select(x => new
            {
                x.Module,
                UserActions = x.UserDescriptor != null ? x.UserDescriptor.GetCanonicalActions().Where(y => y.IsDefined(moduleActionAttributeType, true)).ToDictionary(y => y.ActionName, y => "Общее: " + moduleActionGetDisplayName(y)) : new Dictionary<string, string>(),
                AdminActions = x.AdminDescriptor != null ? x.AdminDescriptor.GetCanonicalActions().Where(y => y.IsDefined(moduleActionAttributeType, true)).ToDictionary(y => y.ActionName, y => "Администрирование: " + moduleActionGetDisplayName(y)) : new Dictionary<string, string>(),
            }).ToDictionary(x => x.Module, x => x.UserActions.Union(x.AdminActions).OrderBy(y => y.Value).ToDictionary(y => y.Key, y => y.Value));

            model.ModulesActions = modulesActions.
                Select(x => new { Group = new SelectListGroup() { Name = x.Key.Caption }, Items = x.Value, Module = x.Key }).
                SelectMany(x => x.Items.Select(y => new SelectListItem() { Text = y.Value, Value = $"{x.Module.IdModule}_{y.Key}", Group = x.Group })).ToList();

            return View("routing_module.cshtml", model);
        }

        [MenuAction("Рассылки и уведомления", "messaging", Module.PERM_MANAGE_MESSAGING)]
        public virtual ActionResult Messaging()
        {
            //Web.AppCore.Get<Core.Messaging.Email.IService>().sendMailFromSite("test", "test@test.ru", "test", "test!!!!!!!!");

            return View("Messaging.cshtml");
        }

        public virtual ActionResult MessagingTestEmail()
        {
            var answer = JsonAnswer();

            try
            {

            }
            catch (Exception ex) { answer.FromException(ex); }

            return ReturnJson(answer);
        }

        [MenuAction("Состояние системы", "monitor")]
        public virtual ActionResult Monitor()
        {
            return View("Monitor.cshtml");
        }

        public virtual ActionResult MonitorJournal(Guid? serviceGuid = null)
        {
            try
            {
                if (!serviceGuid.HasValue) throw new Exception("Не указан идентификатор сервиса.");

                var serviceJournal = AppCore.Get<ServiceMonitor.Monitor>().GetServiceJournal(serviceGuid.Value);
                return View("MonitorJournal.cshtml", serviceJournal.ToList());
            }
            catch (Exception ex)
            {
                var answer = JsonAnswer();
                answer.FromException(ex);
                return ReturnJson(answer);
            }
        }

        [MenuAction("Журналы системы", "journals")]
        public virtual ActionResult Journals()
        {
            using (var db = new JournalingDB.DataContext())
            {
                var dbAccessor = AppCore.Get<JournalingDB.JournalingManagerDatabaseAccessor>();

                //test group
                var queryDataBase = dbAccessor.CreateQueryJournalData(db);
                var queryDataGrouped = from row in queryDataBase
                                       group row.JournalData by row.JournalName.IdJournal into gr
                                       select new { Count = gr.Count(), IdJournalDataLast = gr.Max(x => x.IdJournalData) };

                //test group
                var query = from row in queryDataBase
                            join sq2 in queryDataGrouped on row.JournalData.IdJournalData equals sq2.IdJournalDataLast
                            select new Model.JournalQueries.QueryJournalData
                            {
                                JournalData = row.JournalData,
                                JournalName = row.JournalName,
                                User = row.User,
                                Count = sq2.Count
                            };

                var data = dbAccessor.
                    FetchQueryJournalData<Model.JournalQueries.QueryJournalData, Model.JournalQueries.JournalData>(query, (row, instance) => instance.Count = row.Count).
                    Select(x => new ViewModels.JournalsList()
                    {
                        JournalName = x.JournalInfo,
                        EventsCount = x.Count,
                        LatestEventType = x.EventType,
                        LatestEventDate = x.DateEvent
                    }).ToList();

                return View("Journals.cshtml", data);
            }
        }

        public virtual ActionResult JournalDetails(int? IdJournal = null)
        {
            if (!IdJournal.HasValue) throw new Exception("Не указан идентификатор журнала.");

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadUncommitted }))
                {
                    var result = IdJournal.Value != -1 ? AppCore.Get<JournalingManager>().GetJournal(IdJournal.Value) : new ExecutionResult<Journaling.Model.JournalInfo>();
                    if (IdJournal.Value != -1 && !result.IsSuccess) throw new Exception(result.Message);

                    var dbAccessor = AppCore.Get<JournalingDB.JournalingManagerDatabaseAccessor>();

                    using (var db = new JournalingDB.DataContext())
                    {
                        var query = dbAccessor.CreateQueryJournalData(db);
                        if (IdJournal.Value != -1)
                        {
                            query = query.Where(x => x.JournalData.IdJournal == IdJournal.Value);
                        }
                        var count = query.Count();
                        return View("JournalDetails.cshtml", new ViewModels.JournalDetails()
                        {
                            IdJournal = IdJournal.Value,
                            NameJournal = IdJournal.Value != -1 ? result.Result.Name : "Все журналы",
                            JournalDataCountAll = count
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                var answer = JsonAnswer();
                answer.FromException(ex);
                return ReturnJson(answer, HttpStatusCode.BadRequest);
            }
        }

        public virtual JsonResult JournalDetailsList(int? IdJournal = null, Universal.Pagination.PrimeUiDataTableSourceRequest requestOptions = null)
        {
            if (!IdJournal.HasValue) throw new Exception("Не указан идентификатор журнала.");
            else if (!ModelState.IsValid) throw new Exception("Некорректные параметры запроса.");

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadUncommitted }))
                {
                    var result = IdJournal.Value != -1 ? AppCore.Get<JournalingManager>().GetJournal(IdJournal.Value) : new ExecutionResult<Journaling.Model.JournalInfo>();
                    if (IdJournal.Value != -1 && !result.IsSuccess) throw new Exception(result.Message);

                    var dbAccessor = AppCore.Get<JournalingDB.JournalingManagerDatabaseAccessor>();

                    using (var db = new JournalingDB.DataContext())
                    {
                        var sorted = false;
                        var query = dbAccessor.CreateQueryJournalData(db);
                        if (IdJournal.Value != -1)
                        {
                            query = query.Where(x => x.JournalData.IdJournal == IdJournal.Value);
                        }
                        if (requestOptions != null)
                        {
                            if (requestOptions.FilterFields != null)
                            {
                                foreach (var filter in requestOptions.FilterFields)
                                {
                                    switch (filter.FieldName)
                                    {
                                        case nameof(JournalingDB.QueryJournalData.JournalData.IdJournalData):
                                            if (!int.TryParse(filter.Value, out var idJournalData)) throw new HandledException($"Некорректное значение фильтра для поля '{filter.FieldName}'.");
                                            switch (filter.MatchType)
                                            {
                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.Contains:
                                                    query = query.Where(x => Convert.ToString(x.JournalData.IdJournalData).Contains(idJournalData.ToString()));
                                                    break;

                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.StartsWith:
                                                    query = query.Where(x => Convert.ToString(x.JournalData.IdJournalData).StartsWith(idJournalData.ToString()));
                                                    break;
                                            }
                                            break;

                                        case nameof(JournalingDB.QueryJournalData.JournalData.EventCode):
                                            if (!int.TryParse(filter.Value, out var eventCode)) throw new HandledException($"Некорректное значение фильтра для поля '{filter.FieldName}'.");
                                            switch (filter.MatchType)
                                            {
                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.Contains:
                                                    query = query.Where(x => Convert.ToString(x.JournalData.EventCode).Contains(eventCode.ToString()));
                                                    break;

                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.StartsWith:
                                                    query = query.Where(x => Convert.ToString(x.JournalData.EventCode).StartsWith(eventCode.ToString()));
                                                    break;
                                            }
                                            break;

                                        case nameof(JournalingDB.QueryJournalData.JournalData.EventType):
                                            if (!Enum.TryParse<EventType>(filter.Value, out var eventType)) throw new HandledException($"Некорректное значение фильтра для поля '{filter.FieldName}'.");
                                            switch (filter.MatchType)
                                            {
                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.Contains:
                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.StartsWith:
                                                    query = query.Where(x => x.JournalData.EventType == eventType);
                                                    break;
                                            }
                                            break;

                                        case nameof(JournalingDB.QueryJournalData.JournalData.EventInfo):
                                            switch (filter.MatchType)
                                            {
                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.Contains:
                                                    query = query.Where(x => x.JournalData.EventInfo.Contains(filter.Value));
                                                    break;

                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.StartsWith:
                                                    query = query.Where(x => x.JournalData.EventInfo.StartsWith(filter.Value));
                                                    break;
                                            }
                                            break;

                                        case nameof(JournalingDB.QueryJournalData.JournalData.EventInfoDetailed):
                                            switch (filter.MatchType)
                                            {
                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.Contains:
                                                    query = query.Where(x => x.JournalData.EventInfoDetailed.Contains(filter.Value));
                                                    break;

                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.StartsWith:
                                                    query = query.Where(x => x.JournalData.EventInfoDetailed.StartsWith(filter.Value));
                                                    break;
                                            }
                                            break;

                                        case nameof(JournalingDB.QueryJournalData.JournalData.ExceptionDetailed):
                                            switch (filter.MatchType)
                                            {
                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.Contains:
                                                    query = query.Where(x => x.JournalData.ExceptionDetailed.Contains(filter.Value));
                                                    break;

                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.StartsWith:
                                                    query = query.Where(x => x.JournalData.ExceptionDetailed.StartsWith(filter.Value));
                                                    break;
                                            }
                                            break;

                                        case "EventInfoFull":
                                            switch (filter.MatchType)
                                            {
                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.Contains:
                                                    query = query.Where(x => x.JournalData.EventInfoDetailed.Contains(filter.Value) || x.JournalData.ExceptionDetailed.Contains(filter.Value));
                                                    break;

                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.StartsWith:
                                                    query = query.Where(x => x.JournalData.EventInfoDetailed.StartsWith(filter.Value) || x.JournalData.ExceptionDetailed.StartsWith(filter.Value));
                                                    break;
                                            }
                                            break;

                                        case "JournalName":
                                            switch (filter.MatchType)
                                            {
                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.Contains:
                                                    query = query.Where(x => x.JournalName.Name.Contains(filter.Value));
                                                    break;

                                                case Universal.Pagination.PrimeUiDataTableFieldFilterMatchMode.StartsWith:
                                                    query = query.Where(x => x.JournalName.Name.StartsWith(filter.Value));
                                                    break;
                                            }
                                            break;

                                        default:
                                            throw new HandledException($"Фильтр по полю '{filter.FieldName}' не поддерживается.");

                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(requestOptions.SortByFieldName))
                            {
                                switch (requestOptions.SortByFieldName)
                                {
                                    case nameof(JournalingDB.QueryJournalData.JournalData.IdJournalData):
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ? query.OrderBy(x => x.JournalData.IdJournalData) : query.OrderByDescending(x => x.JournalData.IdJournalData);
                                        break;

                                    case nameof(JournalingDB.QueryJournalData.JournalData.DateEvent):
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ? query.OrderBy(x => x.JournalData.DateEvent) : query.OrderByDescending(x => x.JournalData.DateEvent);
                                        break;

                                    case nameof(JournalingDB.QueryJournalData.JournalData.EventType):
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ? query.OrderBy(x => x.JournalData.EventType) : query.OrderByDescending(x => x.JournalData.EventType);
                                        break;

                                    case nameof(JournalingDB.QueryJournalData.JournalData.EventCode):
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ? query.OrderBy(x => x.JournalData.EventCode) : query.OrderByDescending(x => x.JournalData.EventCode);
                                        break;

                                    case nameof(JournalingDB.QueryJournalData.JournalData.EventInfo):
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ? query.OrderBy(x => x.JournalData.EventInfo) : query.OrderByDescending(x => x.JournalData.EventInfo);
                                        break;

                                    case nameof(JournalingDB.QueryJournalData.JournalData.EventInfoDetailed):
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ?
                                            query.OrderBy(x => x.JournalData.EventInfoDetailed) :
                                            query.OrderByDescending(x => x.JournalData.EventInfoDetailed);
                                        break;

                                    case nameof(JournalingDB.QueryJournalData.JournalData.ExceptionDetailed):
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ?
                                            query.OrderBy(x => x.JournalData.ExceptionDetailed) :
                                            query.OrderByDescending(x => x.JournalData.ExceptionDetailed);
                                        break;

                                    case "EventInfoFull":
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ?
                                            query.OrderBy(x => x.JournalData.EventInfoDetailed).ThenBy(x => x.JournalData.ExceptionDetailed) :
                                            query.OrderByDescending(x => x.JournalData.EventInfoDetailed).ThenByDescending(x => x.JournalData.ExceptionDetailed);
                                        break;

                                    case "JournalName":
                                        sorted = true;
                                        query = requestOptions.SortByAcsending ?
                                            query.OrderBy(x => x.JournalName.Name) :
                                            query.OrderByDescending(x => x.JournalName.Name);
                                        break;

                                }
                            }
                        }

                        var dataAllCount = query.Count();

                        if (requestOptions != null)
                        {
                            if (!sorted) query = query.OrderByDescending(x => x.JournalData.IdJournalData);

                            if (requestOptions.FirstRow > 0) query = query.Skip((int)requestOptions.FirstRow);
                            if (requestOptions.RowsLimit > 0) query = query.Take((int)requestOptions.RowsLimit);
                        }

                        var data = dbAccessor.FetchQueryJournalData(query);
                        return ReturnJson(true, null, new ViewModels.JournalDetails()
                        {
                            JournalDataCountAll = dataAllCount,
                            JournalData = data
                        });
                    }
                }
            }
            catch (HandledException ex)
            {
                RegisterEventWithCode(HttpStatusCode.InternalServerError, "Ошибка при загрузке данных журнала", $"Журнал №{IdJournal}. {ex.Message}");
                return ReturnJson(false, $"Ошибка при загрузке данных журнала. {ex.Message}");
            }
            catch (Exception ex)
            {
                RegisterEventWithCode(HttpStatusCode.InternalServerError, "Неожиданная ошибка при загрузке данных журнала", $"Журнал №{IdJournal}.", ex);
                return ReturnJson(false, "Неожиданная ошибка при загрузке данных журнала");
            }
        }

        public virtual ActionResult JournalClear(int? IdJournal = null)
        {
            var answer = JsonAnswer();

            try
            {
                if (!IdJournal.HasValue) throw new Exception("Не указан идентификатор журнала.");

                var result = AppCore.Get<JournalingManager>().GetJournal(IdJournal.Value);
                if (!result.IsSuccess) throw new Exception(result.Message);

                using (var db = new JournalingDB.DataContext())
                using (var scope = db.CreateScope())
                {
                    db.ExecuteQuery("DELETE FROM Journal WHERE IdJournal=@IdJournal", new { IdJournal = result.Result.IdJournal });
                    scope.Complete();
                }
                answer.FromSuccess(null);
            }
            catch (Exception ex)
            {
                this.RegisterEventWithCode(System.Net.HttpStatusCode.InternalServerError, "Ошибка во время удаления журнала", $"Журнал №{IdJournal}", ex);
                answer.FromFail("Ошибка во время удаления журнала.");
            }
            return ReturnJson(answer);
        }

        [MenuAction("Планировщик задач", null, Module.PERM_TASKSHEDULING)]
        public virtual ActionResult TaskSheduling()
        {
            var taskList = AppCore.Get<TaskSchedulingManager>().GetTaskList(false);
            var model = new ViewModels.TaskSheduling()
            {
                TaskList = taskList.Select(x => new Model.TaskShedulingTask(x)).ToList()
            };
            return View("TaskSheduling.cshtml", model);
        }

        public virtual JsonResult TaskShedulingTaskSave(Model.TaskShedulingTask model = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ReturnJson(false, "Ошибка проверки данных формы.");
                }

                var taskSchedulingManager = AppCore.Get<TaskSchedulingManager>();
                var taskDescription = taskSchedulingManager.GetTaskList(true).Where(x => x.Id == model.Id).FirstOrDefault();
                if (taskDescription == null)
                {
                    return ReturnJson(false, $"Не найдена задача с №{model.Id}.");
                }
                if (taskDescription.IsEnabled != model.IsEnabled)
                {
                    if (!taskDescription.TaskOptions.HasFlag(TaskOptions.AllowDisabling))
                    {
                        return ReturnJson(false, $"Изменение состояния задачи запрещено.");
                    }
                    taskSchedulingManager.SetTaskEnabled(taskDescription, model.IsEnabled);
                }
                if (taskDescription.TaskOptions.HasFlag(TaskOptions.AllowManualSchedule))
                {
                    var scheduleList = new List<TaskSchedule>();
                    if (model.ManualScheduleList != null)
                        foreach(var row in model.ManualScheduleList)
                        {
                            if (row.Type == Model.TaskShedulingScheduleType.Cron) scheduleList.Add(new TaskCronSchedule(row.Cron) { IsEnabled = row.IsEnabled });
                            else if (row.Type == Model.TaskShedulingScheduleType.DateTimeFixed) scheduleList.Add(new TaskFixedTimeSchedule(row.DateTimeFixed.Value) { IsEnabled = row.IsEnabled });
                        }
                    taskSchedulingManager.SetTaskManualScheduleList(taskDescription, scheduleList);
                }

                return ReturnJson(true, null, new Model.TaskShedulingTask(taskDescription));
            }
            catch (HandledException ex)
            {
                RegisterEventWithCode(HttpStatusCode.InternalServerError, "Ошибка во время сохранения задачи", null, ex.InnerException);
                return ReturnJson(false, $"Ошибка во время сохранения задачи.");
            }
            catch (Exception ex)
            {
                RegisterEventWithCode(HttpStatusCode.InternalServerError, "Неожиданная ошибка во время сохранения задачи", null, ex);
                return ReturnJson(false, $"Неожиданная ошибка во время сохранения задачи: {ex.Message}");
            }
        }

        public virtual JsonResult TaskShedulingTaskExecute(int id, bool waitForCompletion = false)
        {
            try
            {
                var taskSchedulingManager = AppCore.Get<TaskSchedulingManager>();
                var taskDescription = taskSchedulingManager.GetTaskList(true).Where(x => x.Id == id).FirstOrDefault();
                if (taskDescription == null)
                {
                    return ReturnJson(false, $"Не найдена задача с №{id}.");
                }
                var task = taskSchedulingManager.ExecuteTask(taskDescription);
                if (waitForCompletion)
                {
                    task.Wait();
                    switch(task.Result)
                    {
                        case TaskExecuted.Executed:
                            return ReturnJson(true, "Задача запущена и завершена.");

                        case TaskExecuted.Faulted:
                            return ReturnJson(false, "Задача запущена и завершена с необработанной ошибкой.");

                        case TaskExecuted.ParallelPrevented:
                            return ReturnJson(false, "Запуск задачи отменен, так как есть другой выполняемый в данный момент экземпляр задачи.");

                        default:
                            throw new InvalidProgramException();
                    }
                }
                else
                {
                    return ReturnJson(true, "Запуск задачи запланирован.");
                }
            }
            catch (HandledException ex)
            {
                RegisterEventWithCode(HttpStatusCode.InternalServerError, "Ошибка во время запуска выполнения задачи", null, ex.InnerException);
                return ReturnJson(false, $"Ошибка во время запуска выполнения задачи.");
            }
            catch (Exception ex)
            {
                RegisterEventWithCode(HttpStatusCode.InternalServerError, "Неожиданная ошибка во время запуска выполнения задачи", null, ex);
                return ReturnJson(false, $"Неожиданная ошибка во время запуска выполнения задачи: {ex.Message}");
            }
        }

    }
}
