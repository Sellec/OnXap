﻿using OnUtils.Architecture.AppCore;
using OnUtils.Architecture.AppCore.DI;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace OnXap
{
    using Binding.Providers;
    using Binding.Routing;
    using Core;
    using Core.Data;
    using Languages;

    /// <summary>
    /// Ядро веб-приложения для ASP.Net MVC.
    /// </summary>
    sealed class OnXApplicationAspNetMvc : OnXApplication
    {
        private readonly Guid _unique = Guid.NewGuid();

        public OnXApplicationAspNetMvc(string physicalApplicationPath, IDbConfigurationBuilder dbConfigurationBuilder) : base(physicalApplicationPath, dbConfigurationBuilder)
        {
        }

        /// <summary>
        /// См. <see cref="AppCore{TAppCore}.OnBindingsRequired(IBindingsCollection{TAppCore})"/>.
        /// </summary>
        protected override void OnBindingsRequired(IBindingsCollection<OnXApplication> bindingsCollection)
        {
            base.OnBindingsRequired(bindingsCollection);

            bindingsCollection.SetSingleton<Core.Storage.ResourceProvider, ResourceProvider>(() =>
            {
                var viewEnginePrevious = ViewEngines.Engines.LastOrDefault(x => !(x is ResourceProvider));
                var instance = new ResourceProvider(viewEnginePrevious);
                ViewEngines.Engines.Insert(0, instance);
                return instance;
            });
            bindingsCollection.SetSingleton<RoutingManager>();
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnApplicationStart()
        {
            if (!(ModelMetadataProviders.Current is TraceModelMetadataProvider))
                ModelMetadataProviders.Current = new TraceModelMetadataProvider();

            ValueProviderFactories.Factories.Insert(0, new TraceJsonValueProviderFactory());

            var controllerFactory = new CustomControllerFactory(ControllerBuilder.Current.GetControllerFactory());
            ((IComponentStartable)controllerFactory).Start(this);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            var routes = RouteTable.Routes;

            routes.LowercaseUrls = true;
            routes.RouteExistingFiles = true;
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Сначала маршруты панели управления, т.к. у них заведомо специфичный адрес.
            var moduleAdmin = Get<Modules.Admin.ModuleAdmin>();

            routes.Clear();

            routes.Add("AdminRoute1", new RouteWithDefaults(
                this,
                moduleAdmin.UrlName + "/mnadmin/{controller}/{action}/{*url}",
                false,
                null,
                new RouteValueDictionary(new { area = AreaConstants.AdminPanel }),
                new MvcRouteHandler(controllerFactory)
            ));

            routes.Add("AdminRoute2", new RouteWithDefaults(
                this,
                moduleAdmin.UrlName + "/madmin/{controller}/{action}/{*url}",
                false,
                null,
                new RouteValueDictionary(new { area = AreaConstants.AdminPanel }),
                new MvcRouteHandler(controllerFactory)
            ));

            // Затем универсальный маршрут для поиска статики и ЧПУ в таблице роутинга.
            var routingHandler = new CustomRouteHandler(this, controllerFactory);

            routes.Add("RoutingAll", new Route(
                "{*url}",
                new RouteValueDictionary(new { controller = "AControllerThatDoesntExists" }),
                new RouteValueDictionary(new { url = routingHandler }),
                routingHandler
            ));

            // Затем маршрут для поиска по пути модуль/action/параметры.
            var languageChecker = new LanguageRouteConstraint(Get<Manager>().GetLanguages()
                                                                            .Where(x => !string.IsNullOrEmpty(x.ShortName))
                                                                            .Select(x => x.ShortName).ToArray());

            routes.Add("DefaultRoute", new RouteWithDefaults(
                this,
                "{controller}/{action}/{*url}",
                true,
                new RouteValueDictionary(new { language = languageChecker, controller = languageChecker, action = languageChecker }),
                new RouteValueDictionary(new { area = AreaConstants.User }),
                new MvcRouteHandler(controllerFactory)
            ));

        }

        #region Свойства
        /// <summary>
        /// См. <see cref="OnXApplication.ServerUrl"/>.
        /// </summary>
        public override Uri ServerUrl
        {
            get { return _serverUrl; }
            set
            {
                _serverUrl = value;
                IsServerUrlHasBeenSet = true;
            }
        }

        /// <summary>
        /// Указывает, был ли адрес сервера уже однажды назначен, т.е. является ли текущее значение <see cref="ServerUrl"/> значением по-умолчанию.
        /// </summary>
        public bool IsServerUrlHasBeenSet { get; private set; } = false;

        private Uri _serverUrl = new Uri("http://localhost");

        /// <summary>
        /// Возвращает уникальный идентификатор приложения.
        /// </summary>
        public Guid ApplicationUniqueId => _unique;
        #endregion
    }
}
