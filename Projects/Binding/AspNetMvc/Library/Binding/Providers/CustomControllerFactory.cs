﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;

namespace OnXap.Binding.Providers
{
    using Core;
    using Core.Modules;
    using Core.Modules.Internal;
    using Exceptions;
    using Routing;

    class CustomControllerFactory : CoreComponentBase, IComponentSingleton, IControllerFactory
    {
        internal static readonly Guid _routingModuleNotFound;
        private readonly IControllerFactory _controllerFactoryOld = null;

        static CustomControllerFactory()
        {
            _routingModuleNotFound = Guid.NewGuid();
        }

        public CustomControllerFactory(IControllerFactory controllerFactoryOld)
        {
            _controllerFactoryOld = controllerFactoryOld;
        }

        #region IControllerFactory
        public IController CreateController(RequestContext requestContext, string moduleName)
        {
            requestContext.HttpContext.Items["TimeController"] = DateTime.Now;

            var isAjax = false;

            // Проверка на авторизацию. Ловим случаи, когда авторизация не сработала в HttpApplication.
            var context = AppCore.GetUserContextManager().GetCurrentUserContext();
            if (context.IsGuest)
            {
                var sessionBinder = AppCore.Get<SessionBinder>();
                context = sessionBinder.RestoreUserContextFromRequest();
                if (context != null && !context.IsGuest)
                {
                    AppCore.GetUserContextManager().SetCurrentUserContext(context);
                }
            }

            IModuleCore module = null;

            var lambda = new Func<Exception, IController>(ex =>
            {
                try
                {
                    if (module == null)
                    {
                        var moduleTmp = new ModuleInternalErrors();
                        ((IComponentStartable)moduleTmp).Start(AppCore);
                        module = moduleTmp;
                    }

                    var type = typeof(ModuleControllerInternalErrors<>).MakeGenericType(module.QueryType);
                    var controller = CreateController(module, type, requestContext.RouteData.Values);
                    (controller as IModuleControllerInternalErrors).SetException(ex);
                    // todo (controller as Modules.ModuleController).IsAdminController = isErrorAdmin;

                    HttpContext.Current.Items["RequestContextController"] = controller;
                    return controller;
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine("Throw: {0}", ex2.ToString());
                    throw ex;
                }
            });

            try
            {
                /*
                * Определение языка и темы
                */
                {
                    var lang = string.Format("{0}", requestContext.RouteData.Values["language"]);

                    using (var db = new Languages.DB.DataContext())
                    {
                        var query = from Language in db.Language
                                    where Language.IsDefault != 0 || Language.ShortAlias == lang
                                    orderby (Language.ShortAlias == lang ? 1 : 0) descending
                                    select Language;

                        var data = query.ToList();
                        if (data.Count > 0)
                        {
                            var res = data.First();
                            requestContext.RouteData.Values["language"] = res.ShortAlias;
                        }
                    }
                }

                if (moduleName == _routingModuleNotFound.ToString())
                {
                    var error = requestContext.HttpContext.Items[_routingModuleNotFound.ToString() + "_RoutingError"] as string;
                    return lambda(new ErrorCodeException(HttpStatusCode.NotFound, error));
                }
                else
                {
                    /*
                     * Ищем модуль, к которому обращаются запросом.
                     * */
                    if (int.TryParse(moduleName, out int moduleId) && moduleId.ToString() == moduleName)
                        module = (IModuleCore)AppCore.GetModulesManager().GetModule(moduleId);
                    else if (Guid.TryParse(moduleName, out Guid uniqueName) && uniqueName.ToString() == moduleName)
                        module = (IModuleCore)AppCore.GetModulesManager().GetModule(uniqueName);
                    else
                        module = (IModuleCore)AppCore.GetModulesManager().GetModule(moduleName);

                    if (module == null) return lambda(new ErrorCodeException(HttpStatusCode.NotFound, $"Адрес '{moduleName}' не найден."));
                }

                /*
                 * Ищем контроллер, который относится к модулю.
                 * */
                var controllerType = ControllerTypeFactory.RoutingPrepareURL(requestContext.HttpContext.Request, UriExtensions.MakeRelativeFromUrl(requestContext.HttpContext.Request.Url.PathAndQuery));

                if (requestContext.RouteData.Route is Route)
                {
                    /*
                     * Анализируем адрес и устанавливаем признак, если это вызов в панель управления. Пришлось пойти на такой хак.
                     * */
                    var route = requestContext.RouteData.Route as Route;
                    if (route.Url.StartsWith("admin/madmin")) isAjax = true;

                    if (isAjax) HttpContext.Current.Items["isAjax"] = true;
                }
                
                var controller = CreateController(controllerType, module, requestContext.RouteData.Values);
                HttpContext.Current.Items["RequestContextController"] = controller;
                return controller;
            }
            catch (Exception ex)
            {
                return lambda(ex);
            }
        }

        private IController CreateController(ControllerType controllerType, IModuleCore module, RouteValueDictionary routeValues)
        {
            var controllerTypes = AppCore.Get<ModuleControllerTypesManager>().GetModuleControllerTypes(module.QueryType);
            var targetType = controllerTypes.GetValueOrDefault(controllerType.ControllerTypeID);
            if (targetType == null) throw new NotSupportedException(controllerType.ErrorCannotFindControllerTypeSpecified(module, routeValues));

            if (!controllerType.CheckPermissions(module, routeValues))
            {
                throw new ErrorCodeException(HttpStatusCode.Forbidden, "Отсутствует доступ.");
            }

            if (targetType != null)
            {
                return CreateController(module, targetType, routeValues);
            }

            return null;
        }

        private IController CreateController(IModuleCore module, Type controllerType, RouteValueDictionary routeValues)
        {
            var controller = (ModuleControllerBase)DependencyResolver.Current.GetService(controllerType);
            if (controller == null) throw new Exception($"Контроллер для модуля '{module.UrlName}' не найден.");

            controller.Start(AppCore);

            var method = controller.GetType().GetMethod("InitController", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance);
            method.Invoke(controller, new object[] { module });

            var methods = controller.GetType().GetMethods();
            var action = (routeValues["action"]?.ToString() ?? "").ToLower();
            method = (from p in methods where p.Name.ToLower() == action select p).FirstOrDefault();
            if (method == null)
            {
                foreach (var mm in methods)
                {
                    var attrs = mm.GetCustomAttributes(typeof(ModuleActionAttribute), true);
                    if (attrs != null && attrs.Length > 0)
                    {
                        var attr = attrs.First() as ModuleActionAttribute;
                        if (attr.Alias?.ToLower() == action)
                        {
                            routeValues["action"] = mm.Name;
                            method = mm;
                            break;
                        }
                    }
                }
            }

            if (method != null && routeValues["url"] != null)
            {
                var parameters = method.GetParameters();

                var url = routeValues["url"].ToString();
                // url = url.Truncate(0, url.IndexOf('?'));
                var parts = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    int idx = 0;
                    foreach (var param in parameters)
                    {
                        if (!routeValues.ContainsKey(param.Name))
                        {
                            routeValues[param.Name] = parts[idx];
                        }

                        idx++;
                        if (idx >= parts.Length) break;
                    }
                }
            }

            return controller;
        }

        public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
        {
            return SessionStateBehavior.ReadOnly;
        }

        public void ReleaseController(IController controller)
        {
            if (controller is IDisposable disposable) disposable.Dispose();
        }

        #endregion
    }

}
