using OnUtils;
using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;
using System;

namespace OnXap
{
    using Core.Configuration;
    using Core.Modules;
    using Modules.CoreModule;
    using Modules.WebCoreModule;
    using Users;

    /// <summary>
    /// Ядро приложения.
    /// </summary>
    public abstract class OnXApplication : AppCore<OnXApplication> 
    {
        private CoreConfiguration _appConfigurationAccessor = null;
        private WebCoreConfiguration _webConfigurationAccessor = null;

        /// <summary>
        /// Создает новый экземпляр приложения.
        /// </summary>
        /// <exception cref="ArgumentNullException">Возникает, если передана пустая строка подключения к базе данных приложения (<paramref name="connectionString"/>).</exception>
        public OnXApplication(string physicalApplicationPath, string connectionString) : this(physicalApplicationPath, () => connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Создает новый экземпляр приложения.
        /// </summary>
        /// <exception cref="ArgumentNullException">Возникает, если передана пустая фабрика строк подключения к базе данных приложения (<paramref name="applicationConnectionStringFactory"/>).</exception>
        public OnXApplication(string physicalApplicationPath, Func<string> applicationConnectionStringFactory)
        {
            if (!GetType().Assembly.FullName.EndsWith("")) throw new InvalidProgramException("");
            if (applicationConnectionStringFactory == null) throw new ArgumentNullException(nameof(applicationConnectionStringFactory));

            try
            {
                LibraryEnumeratorFactory.LibraryDirectory = physicalApplicationPath;
                ApplicationWorkingFolder = physicalApplicationPath;

                ConnectionStringFactory = applicationConnectionStringFactory;
                Core.Db.CoreContextBase.ConnectionStringFactory = applicationConnectionStringFactory;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error init ApplicationCore: {0}", ex.ToString());
                if (ex.InnerException != null) Debug.WriteLine("Error init ApplicationCore inner: {0}", ex.InnerException.Message);
                if (ex.InnerException?.InnerException != null) Debug.WriteLine("Error init ApplicationCore inner inner: {0}", ex.InnerException.InnerException.Message);
                if (ex.InnerException?.InnerException?.InnerException != null) Debug.WriteLine("Error init ApplicationCore inner inner inner: {0}", ex.InnerException.InnerException.InnerException.Message);

                throw;
            }
        }

        #region Методы
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
            try
            {
                OnApplicationStartBase();
            }
            catch (Exception ex)
            {
                if (AppDebugLevel >= DebugLevel.Common)
                    Debug.WriteLine($"{nameof(OnXApplication)}.{nameof(OnApplicationStartBase)}: ошибка во время запуска.");
                if (AppDebugLevel >= DebugLevel.Detailed)
                    Debug.WriteLine($"{nameof(OnXApplication)}.{nameof(OnApplicationStartBase)}: {ex}");
            }

            try
            {
                OnApplicationStart();
            }
            catch (Exception ex)
            {
                if (AppDebugLevel >= DebugLevel.Common)
                    Debug.WriteLine($"{nameof(OnXApplication)}.{nameof(OnApplicationStart)}: ошибка во время запуска.");
                if (AppDebugLevel >= DebugLevel.Detailed)
                    Debug.WriteLine($"{nameof(OnXApplication)}.{nameof(OnApplicationStart)}: {ex}");
            }

            DeprecatedApplicationHolder.Set(this);
        }

        private void OnApplicationStartBase()
        {
            // Проверка ошибок применения схемы
            Get<Core.DbSchema.DbSchemaManager>().WriteErrors();
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
            if (AppDebugLevel >= DebugLevel.Common)
                Debug.WriteLine($"{nameof(OnXApplication)}.{nameof(OnStop)}");
            DeprecatedApplicationHolder.Remove(this);
        }

        /// <summary>
        /// Вызывается единственный раз при запуске ядра.
        /// </summary>
        protected virtual void OnApplicationStart()
        {
        }

        /// <summary>
        /// См. <see cref="AppCore{TAppCore}.OnBindingsApplied"/>.
        /// </summary>
        protected sealed override void OnBindingsApplied()
        {
        }

        /// <summary>
        /// См. <see cref="AppCore{TAppCore}.OnBindingsRequired(IBindingsCollection{TAppCore})"/>.
        /// </summary>
        protected override void OnBindingsRequired(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            if (AppDebugLevel >= DebugLevel.Common)
                Debug.WriteLine($"{nameof(OnXApplication)}.{nameof(OnBindingsRequired)}");

            bindingsCollection.SetSingleton<ApplicationLauncher>();
            bindingsCollection.SetSingleton<Core.Items.ItemsManager>();
            bindingsCollection.SetSingleton<Journaling.JournalingManager>();
            bindingsCollection.SetSingleton<Journaling.DB.JournalingManagerDatabaseAccessor>();
            bindingsCollection.SetSingleton<Messaging.MessagingManager>();
            bindingsCollection.SetSingleton<ModulesManager>();
            bindingsCollection.SetSingleton<ModulesLoadStarter>();
            bindingsCollection.SetSingleton<ServiceMonitor.Monitor>();
            bindingsCollection.SetSingleton<UserContextManager>();
            bindingsCollection.SetSingleton<Languages.Manager>();
        }

        /// <summary>
        /// </summary>
        protected sealed override IBindingsResolver<OnXApplication> GetBindingsResolver()
        {
            return new Core.MetadataObject.MetadataObjectDbContextResolver();
        }
        #endregion

        #region Упрощение доступа
        /// <summary>
        /// Возвращает менеджер модулей для приложения.
        /// </summary>
        public ModulesManager GetModulesManager()
        {
            return Get<ModulesManager>();
        }

        /// <summary>
        /// Возвращает менеджер контекстов пользователя для приложения.
        /// </summary>
        public UserContextManager GetUserContextManager()
        {
            return Get<UserContextManager>();
        }

        private CoreModule GetCoreModule()
        {
            return GetModulesManager().GetModule<CoreModule>();
        }

        #endregion

        #region Свойства
        /// <summary>
        /// Основные настройки приложения.
        /// </summary>
        public CoreConfiguration AppConfig
        {
            get
            {
                if (_appConfigurationAccessor == null) _appConfigurationAccessor = GetCoreModule().GetConfiguration<CoreConfiguration>();
                return _appConfigurationAccessor;
            }
        }

        /// <summary>
        /// Возвращает рабочую директорию приложения. 
        /// </summary>
        public string ApplicationWorkingFolder { get; private set; }

        /// <summary>
        /// Возвращает модуль ядра приложения.
        /// </summary>
        public CoreModule AppCoreModule
        {
            get => Get<CoreModule>();
        }

        /// <summary>
        /// Возвращает основной веб-модуль приложения.
        /// </summary>
        public WebCoreModule WebCoreModule
        {
            get => Get<WebCoreModule>();
        }

        /// <summary>
        /// Основные настройки веб-приложения.
        /// </summary>
        public WebCoreConfiguration WebConfig
        {
            get
            {
                if (_webConfigurationAccessor == null) _webConfigurationAccessor = Get<WebCoreModule>().GetConfiguration<WebCoreConfiguration>();
                return _webConfigurationAccessor;
            }
        }

        /// <summary>
        /// Возвращает строку подключения. todo - разобраться со строками подключений.
        /// </summary>
        public Func<string> ConnectionStringFactory { get; private set; }

        /// <summary>
        /// Внешний URL-адрес сервера.
        /// </summary>
        public virtual Uri ServerUrl
        {
            get;
            set;
        } = new Uri("http://localhost");
        #endregion
    }
}
