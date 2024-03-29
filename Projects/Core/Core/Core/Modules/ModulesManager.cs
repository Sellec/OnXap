﻿using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnXap.Core.Modules
{
    using Configuration;
    using Db;
    using Journaling;

    /// <summary>
    /// Менеджер, управляющий модулями системы.
    /// Система разделена на модули с определенным функционалом, к модулям могут быть привязаны операции, доступные пользователю извне (для внешних запросов).
    /// Права доступа регистрируются на модуль.
    /// </summary>
    public class ModulesManager :
        CoreComponentBase,
        IComponentSingleton,
        IAutoStart,
        ITypedJournalComponent<ModulesManager>
    {
        class InstanceActivatingHandlerImpl : IInstanceActivatingHandler
        {
            private readonly ModulesManager _manager;

            public InstanceActivatingHandlerImpl(ModulesManager manager)
            {
                _manager = manager;
            }

            void IInstanceActivatingHandler.OnInstanceActivating<TRequestedType>(object instance)
            {
                if (instance is ModuleCore moduleCandidate)
                {
                    _manager.GetType().GetMethod(nameof(LoadModule), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(typeof(TRequestedType)).Invoke(_manager, new object[] { instance });
                }
            }
        }

        class InstanceActivatedHandlerImpl : IInstanceActivatedHandler
        {
            private readonly ModulesManager _manager;

            public InstanceActivatedHandlerImpl(ModulesManager manager)
            {
                _manager = manager;
            }

            void IInstanceActivatedHandler.OnInstanceActivated<TRequestedType>(object instance)
            {
                if (instance is ModuleCore moduleActivated)
                {
                    moduleActivated.OnModuleStarted();
                }
            }
        }

        private readonly object _syncRoot = new object();
        private List<Tuple<Type, ModuleCore>> _modules = new List<Tuple<Type, ModuleCore>>();
        private readonly InstanceActivatingHandlerImpl _instanceActivatingHandler = null;
        private readonly InstanceActivatedHandlerImpl _instanceActivatedHandler = null;

        /// <summary>
        /// Создает новый экземпляр менеджера модулей.
        /// </summary>
        public ModulesManager()
        {
            DeprecatedSingletonInstances.Set(this);
            _instanceActivatingHandler = new InstanceActivatingHandlerImpl(this);
            _instanceActivatedHandler = new InstanceActivatedHandlerImpl(this);
        }

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStarting()
        {
            this.RegisterJournal("Менеджер модулей");

            AppCore.ObjectProvider.RegisterInstanceActivatingHandler(_instanceActivatingHandler);
            AppCore.ObjectProvider.RegisterInstanceActivatedHandler(_instanceActivatedHandler);
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion

        #region Методы
        /// <summary>
        /// Ищет и запускает все модули, для которых зарегистрированы привязки типов.
        /// </summary>
        internal protected void StartModules()
        {
            if (AppCore.AppDebugLevel >= DebugLevel.Common)
                Debug.WriteLine($"{nameof(ModulesManager)}.{nameof(StartModules)}");

            lock (_syncRoot)
            {
                var moduleCoreType = typeof(ModuleCore);

                // Сначала ищем список модулей.
                var typesList = AppCore.GetQueryTypes().ToList();
                var filteredTypesList = typesList.Where(FilterModuleTypes).ToList();
                if (!filteredTypesList.IsNullOrEmpty() && filteredTypesList.Any(type => !typeof(ModuleCore).IsAssignableFrom(type)))
                    throw new ApplicationStartException(ApplicationStartStep.BindingsAutoStartCritical, typeof(ModulesManager), new ArgumentException());

                if (AppCore.AppDebugLevel >= DebugLevel.Detailed)
                {
                    Debug.WriteLine($"{nameof(ModulesManager)}.{nameof(StartModules)}: список всех query-типов (всего {typesList.Count}):");
                    typesList.OrderBy(x => x.FullName).ForEach(x => Debug.WriteLine($"{nameof(ModulesManager)}.{nameof(StartModules)}: {x}"));
                    Debug.WriteLine($"{nameof(ModulesManager)}.{nameof(StartModules)}: список отфильтрованных query-типов (всего {filteredTypesList.Count}):");
                    filteredTypesList.OrderBy(x => x.FullName).ForEach(x => Debug.WriteLine($"{nameof(ModulesManager)}.{nameof(StartModules)}: {x}"));
                }

                foreach (var moduleType in filteredTypesList)
                {
                    try
                    {
                        if (AppCore.AppDebugLevel >= DebugLevel.Detailed)
                            Debug.WriteLine($"{nameof(ModulesManager)}.{nameof(StartModules)}: запуск модуля '{moduleType}'");

                        var moduleInstance = AppCore.Get<ModuleCore>(moduleType);
                    }
                    catch (Exception ex)
                    {
                        if (AppCore.AppDebugLevel >= DebugLevel.Detailed)
                        {
                            Debug.WriteLine($"{nameof(ModulesManager)}.{nameof(StartModules)}: ошибка запуска модуля");
                            Debug.WriteLine($"{nameof(ModulesManager)}.{nameof(StartModules)}: {ex}");
                        }

                        if (typeof(ICritical).IsAssignableFrom(moduleType)) throw new ApplicationStartException(ApplicationStartStep.BindingsAutoStartCritical, moduleType, ex);
                    }
                }
            }
        }

        /// <summary>
        /// Используется для определения того, является ли тип <paramref name="typeFromDI"/> типом модуля. По-умолчанию возвращает только типы, наследующие <see cref="ModuleCore{TSelfReference}"/>. Может быть перегружен для изменения поиска типов.
        /// свойство <see cref="ApplicationStartException.Step"/> будет равно <see cref="ApplicationStartStep.BindingsAutoStartCritical"/>, 
        /// свойство <see cref="ApplicationStartException.ContextType"/> будет равно <see cref="ModulesManager"/>, 
        /// свойство <see cref="Exception.InnerException"/> будет содержать исключение <see cref="ArgumentException"/>.
        /// </summary>
        protected bool FilterModuleTypes(Type typeFromDI)
        {
            var moduleCoreType = typeof(ModuleCore);
            return moduleCoreType.IsAssignableFrom(typeFromDI) && typeFromDI.GetCustomAttribute<ModuleCoreAttribute>() != null;
        }

        private void LoadModule<TModuleType>(TModuleType module)
        {
            GetType().GetMethod(nameof(LoadModuleCustom), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(typeof(TModuleType)).Invoke(this, new object[] { module });
        }

        private void LoadModuleCustom<TModuleType>(TModuleType module)
            where TModuleType : ModuleCore<TModuleType>
        {
            var fullName = typeof(TModuleType).FullName;

            try
            {
                if (AppCore.AppDebugLevel >= DebugLevel.Common)
                    Debug.WriteLine($"{nameof(ModulesManager)}.{nameof(LoadModuleCustom)}: загрузка модуля '{typeof(TModuleType)}'.");

                var moduleType = typeof(TModuleType);
                var moduleCoreAttribute = moduleType.GetCustomAttribute<ModuleCoreAttribute>();

                var moduleRegisterHandlerTypes = AppCore.GetQueryTypes().Where(x => typeof(IModuleRegisteredHandler).IsAssignableFrom(x)).ToList();
                var moduleRegisterHandlers = moduleRegisterHandlerTypes.Select(x => AppCore.Get<IModuleRegisteredHandler>(x)).ToList();

                using (var db = new CoreContext())
                {
                    fullName = Utils.TypeNameHelper.GetFullNameCleared(moduleType);

                    if (AppCore.AppDebugLevel >= DebugLevel.Detailed)
                        Debug.WriteLine($"{nameof(ModulesManager)}.{nameof(LoadModuleCustom)}: поиск настроек.");

                    var config = db.Module.Where(x => x.UniqueKey == fullName).FirstOrDefault();
                    if (config == null)
                    {
                        config = new ModuleConfig() { UniqueKey = fullName, DateChange = DateTime.Now };
                        db.Module.Add(config);
                        db.SaveChanges();
                    }

                    if (AppCore.AppDebugLevel >= DebugLevel.Detailed)
                        Debug.WriteLine($"{nameof(ModulesManager)}.{nameof(LoadModuleCustom)}: применение настроек.");

                    module.ID = config.IdModule;
                    module._moduleCaption = moduleCoreAttribute.Caption;
                    module._moduleUrlName = moduleCoreAttribute.DefaultUrlName;

                    var configurationManipulator = new ModuleConfigurationManipulator<TModuleType>(module, CreateValuesProviderForModule(module));
                    ((IComponentStartable)configurationManipulator).Start(AppCore);
                    module._configurationManipulator = configurationManipulator;

                    var cfg = configurationManipulator.GetUsable<ModuleConfiguration<TModuleType>>();

                    if (AppCore.AppDebugLevel >= DebugLevel.Detailed)
                        Debug.WriteLine($"{nameof(ModulesManager)}.{nameof(LoadModuleCustom)}: инициализация.");

                    if (!string.IsNullOrEmpty(cfg.UrlName)) module._moduleUrlName = cfg.UrlName;
                    module.InitModule();
                    moduleRegisterHandlers.ForEach(x => x.OnModuleInitialized<TModuleType>(module));

                    _modules.RemoveAll(x => x.Item1 == typeof(TModuleType));
                    _modules.Add(new Tuple<Type, ModuleCore>(typeof(TModuleType), module));

                    AppCore.Get<JournalingManager>().RegisterJournalTyped<TModuleType>($"Журнал событий модуля '{module.Caption}'");

                    this.RegisterEvent(
                         EventType.Info,
                        $"Загрузка модуля '{fullName}'",
                        $"Модуль загружен на основе типа '{module.GetType().FullName}' с Id={config.IdModule}."
                    );

                    if (AppCore.AppDebugLevel >= DebugLevel.Detailed)
                        Debug.WriteLine($"{nameof(ModulesManager)}.{nameof(LoadModuleCustom)}: успешно.");
                }
            }
            catch (Exception ex)
            {
                if (AppCore.AppDebugLevel >= DebugLevel.Common)
                    Debug.WriteLine($"{nameof(ModulesManager)}.{nameof(LoadModuleCustom)}: ошибка.");
                if (AppCore.AppDebugLevel >= DebugLevel.Detailed)
                    Debug.WriteLine($"{nameof(ModulesManager)}.{nameof(LoadModuleCustom)}: {ex}");

                this.RegisterEvent(
                     EventType.Error,
                    $"Загрузка модуля '{fullName}'",
                    $"Ошибка загрузки.",
                    ex
                );
                throw;
            }
        }

        /// <summary>
        /// Возвращает модуль указанного типа <typeparamref name="TModule"/>, если таковой имеется среди загруженных. 
        /// </summary>
        /// <typeparam name="TModule">Тип модуля. Это должен быть query-тип из привязок типов (см. описание <see cref="OnXApplication"/>).</typeparam>
        /// <returns>Объект модуля либо null, если подходящий модуль не найден.</returns>
        public TModule GetModule<TModule>(bool isSearchNested = true) where TModule : ModuleCore
        {
            lock (_syncRoot)
            {
                return _modules.Where(x => x.Item1 == typeof(TModule)).Select(x => x.Item2 as TModule).FirstOrDefault();
            }
        }

        /// <summary>
        /// Возращает список модулей, зарегистрированных в системе.
        /// </summary>
        public List<ModuleCore> GetModules()
        {
            lock (_syncRoot)
            {
                var module = _modules.
                    Select(x => (ModuleCore)x.Item2).
                    ToList();

                return module;
            }
        }

        /// <summary>
        /// Возвращает модуль с url-доступным именем <paramref name="urlName"/> (см. <see cref="ModuleCore.UrlName"/>).
        /// </summary>
        /// <returns>Объект модуля либо null, если подходящий модуль не найден.</returns>
        public ModuleCore GetModule(string urlName)
        {
            lock (_syncRoot)
            {
                // Сначала ищем имя модуля среди сохраненных имен модулей. Если совсем ничего нет - то по UrlName, который в случае пустого сохраненного имени хранит гуид.
                var module =
                    _modules.
                    Select(x => x.Item2).
                    Where(x => !string.IsNullOrEmpty(x._moduleUrlName) && x._moduleUrlName.Equals(urlName, StringComparison.InvariantCultureIgnoreCase)).
                    FirstOrDefault() ??
                    _modules.
                    Select(x => x.Item2).
                    Where(x => !string.IsNullOrEmpty(x.UrlName) && x.UrlName.Equals(urlName, StringComparison.InvariantCultureIgnoreCase)).
                    FirstOrDefault();

                return module;
            }
        }

        /// <summary>
        /// Возвращает модуль с идентификатором <paramref name="moduleID"/> (см. <see cref="ModuleCore.IdModule"/>). 
        /// </summary>
        /// <param name="moduleID">Идентификатор модуля.</param>
        /// <returns>Объект модуля либо null, если подходящий модуль не найден.</returns>
        public ModuleCore GetModule(int moduleID)
        {
            lock (_syncRoot)
            {
                var module = _modules.
                    Select(x => x.Item2).
                    Where(x => x.IdModule == moduleID).
                    FirstOrDefault();

                return module;
            }
        }

        /// <summary>
        /// Возвращает модуль с уникальным именем <paramref name="uniqueName"/> (см. <see cref="ModuleCore.UniqueName"/>). 
        /// </summary>
        /// <param name="uniqueName">Уникальное имя модуля.</param>
        /// <returns>Объект модуля либо null, если подходящий модуль не найден.</returns>
        public ModuleCore GetModule(Guid uniqueName)
        {
            lock (_syncRoot)
            {
                var module = _modules.
                    Select(x => x.Item2).
                    Where(x => x.UniqueName == uniqueName).
                    FirstOrDefault();

                return module;
            }
        }

        internal ConfigurationValuesProvider CreateValuesProviderForModule(ModuleCore module)
        {
            var configurationValuesProvider = new ConfigurationValuesProvider();
            using (var db = new CoreContext())
            {
                var config = db.Module.FirstOrDefault(x => x.IdModule == module.ID);
                if (config != null)
                {
                    configurationValuesProvider.Load(config.Configuration);
                }
            }
            return configurationValuesProvider;
        }

        internal ApplyConfigurationResult ApplyModuleConfiguration<TModule, TConfiguration>(TConfiguration configuration, ModuleConfigurationManipulator<TModule> moduleConfigurationManipulator, TModule module)
            where TModule : ModuleCore<TModule>
            where TConfiguration : ModuleConfiguration<TModule>, new()
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var context = AppCore.GetUserContextManager().GetCurrentUserContext();

            var permissionCheck = module.CheckPermission(context, ModulesConstants.PermissionSaveConfiguration);
            if (permissionCheck == CheckPermissionVariant.Denied) return ApplyConfigurationResult.PermissionDenied;

            var moduleType = typeof(TModule);
            var moduleCoreAttribute = moduleType.GetCustomAttribute<ModuleCoreAttribute>();

            var urlNameEncoded = System.Web.HttpUtility.UrlEncode(configuration.UrlName);
            configuration.UrlName = urlNameEncoded;

            using (var db = new CoreContext())
            using (var scope = db.CreateScope())
            {
                var moduleConfig = db.Module.FirstOrDefault(x => x.IdModule == module.ID);
                if (moduleConfig == null)
                {
                    var fullName = Utils.TypeNameHelper.GetFullNameCleared(typeof(TModule));
                    moduleConfig = db.Module.FirstOrDefault(x => x.UniqueKey == fullName);
                    if (moduleConfig == null)
                    {
                        moduleConfig = new ModuleConfig() { UniqueKey = fullName, DateChange = DateTime.Now, IdUserChange = 0 };
                        db.Module.Add(moduleConfig);
                    }
                }

                moduleConfig.Configuration = configuration._valuesProvider.Save();
                moduleConfig.DateChange = DateTime.Now;
                moduleConfig.IdUserChange = context.IdUser;

                db.SaveChanges();
                scope.Complete();

                module.ID = moduleConfig.IdModule;
                module._moduleUrlName = string.IsNullOrEmpty(configuration.UrlName) ? moduleCoreAttribute.DefaultUrlName : configuration.UrlName;
                moduleConfigurationManipulator._valuesProviderUsable.Load(moduleConfig.Configuration);
            }

            return ApplyConfigurationResult.Success;
        }

        #endregion
    }
}
