﻿using OnUtils.Architecture.AppCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using System.Web.Routing;

namespace OnXap.Core.Modules
{
    using Binding.Routing;
    using Core;
    using Users;
    using Exceptions;
    using Journaling;

    /// <summary>
    /// Базовый класс контроллера. Не должен использоваться для создания контроллеров напрямую, только через наследование от <see cref="ModuleControllerBase"/>
    /// </summary>
    [ModuleController(ControllerTypeDefault.TypeID)]
    public abstract class ModuleControllerBase : Controller, IComponentTransient
    {
        private class CoreComponentBaseImpl : CoreComponentBase
        {
            protected override void OnStarting()
            {
            }

            protected override void OnStop()
            {
            }
        }

        private static ConcurrentDictionary<int, int> _journalsForErrors = new ConcurrentDictionary<int, int>();
        private CoreComponentBaseImpl _coreComponent = new CoreComponentBaseImpl();

        /// <summary>
        /// Возвращает идентификатор журнала для записи ошибок с соответствующим кодом <paramref name="errorCode"/>.
        /// </summary>
        public int GetJournalForErrors(int errorCode)
        {
            var journalName = "Журнал ошибок для кода HTTP " + errorCode;
            switch(errorCode)
            {
                case 406:
                    journalName += ": Некорректное заполнение форм";
                    break;

                case 530:
                    journalName += ": Время открытия страниц";
                    break;

            }

            var result = AppCore.Get<JournalingManager>().RegisterJournal(JournalingConstants.IdSystemJournalType, journalName, "ModuleControllerError_" + errorCode.ToString());
            if (!result.IsSuccess) Debug.WriteLine("Ошибка получения журнала для кода {0}: {1}", errorCode, result.Message);
            return result.Result?.IdJournal ?? -1;
        }

        internal ModuleControllerBase()
        {
            var extractedType = OnUtils.Types.TypeHelpers.ExtractGenericType(this.GetType(), typeof(ModuleControllerUser<>));
            if (extractedType == null) throw new TypeAccessException($"Тип '{typeof(ModuleControllerBase).FullName}' должен находиться в цепочке наследования после '{typeof(ModuleControllerUser<>).FullName}' или '{typeof(ModuleControllerUser<>).FullName}'");
        }

        #region ICoreComponent
        /// <summary>
        /// См. <see cref="IComponentStartable{TAppCore}.Start(TAppCore)"/>.
        /// </summary>
        public void Start(OnXApplication core)
        {
            ((IComponentStartable)_coreComponent).Start(core);
        }

        /// <summary>
        /// См. <see cref="IComponent{TAppCore}.Stop"/>.
        /// </summary>
        public void Stop()
        {
            ((IComponent)_coreComponent).Stop();
        }

        /// <summary>
        /// См. <see cref="IComponent{TAppCore}.GetState"/>.
        /// </summary>
        public CoreComponentState GetState()
        {
            return _coreComponent.GetState();
        }

        /// <summary>
        /// См. <see cref="IComponent{TAppCore}.GetAppCore"/>.
        /// </summary>
        public OnXApplication GetAppCore()
        {
            return _coreComponent.GetAppCore();
        }
        #endregion

        #region Методы для переопределения в контроллерах.
        /// <summary>
        /// Вызывается перед вызовом метода контроллера. Должен использоваться вместо <see cref="OnActionExecuting(ActionExecutingContext)"/>, т.к. <see cref="OnActionExecuting(ActionExecutingContext)"/> запечатан.
        /// </summary>
        /// <param name="filterContext">Информация о текущем запросе и методе.</param>
        protected virtual void OnBeforeExecution(ActionExecutingContext filterContext)
        {

        }

        /// <summary>
        /// Вызывается после вызова метода контроллера. Должен использоваться вместо <see cref="OnActionExecuted(ActionExecutedContext)"/>, т.к. <see cref="OnActionExecuted(ActionExecutedContext)"/> запечатан.
        /// </summary>
        /// <param name="filterContext">Информация о текущем запросе и методе.</param>
        protected virtual void OnAfterExecution(ActionExecutedContext filterContext)
        {

        }

        /// <summary>
        /// Вызывается при возникновении ошибки в контролере во время обработки запроса.
        /// Обрабатываются:
        ///  - неперехваченные исключения во время работы контроллера;
        ///  - ошибка HandleUnknownAction (передается <see cref="ErrorCodeException"/> с кодом 404). 
        /// </summary>
        /// <param name="exception">Возникшее исключение.</param>
        /// <returns></returns>
        public virtual ActionResult ErrorHandled(Exception exception)
        {
            var exceptionType = exception.GetType();

            int code = 500;
            if (exception is ErrorCodeException exc) code = (int)exc.Code;

            Response.StatusCode = code;
            Response.TrySkipIisCustomErrors = true;

            if (Types.RequestAnswerType.GetAnswerType() == Types.RequestAnswerType.Types.Json)
            {
                return ReturnJson(false, exception != null ? exception.Message : "Неизвестная ошибка");
            }

            if (code == 404)
                return View("error404NotFound.cshtml", exception);
            else if (code == 401)
                return View("error401NotAuthorized.cshtml", exception);
            else if (code == 403)
                return View("error403NotAllowed.cshtml", exception);
            else
                return View("errorHandled.cshtml", exception);
        }

        #endregion

        #region Переопределение стандартного поведения контроллеров ASP.NET MVC.
        /// <summary>
        /// Запечатан.
        /// </summary>
        protected sealed override IActionInvoker CreateActionInvoker()
        {
            var baseInvoker = base.CreateActionInvoker();
            if (baseInvoker is IAsyncActionInvoker) return new ModuleActionAsyncInvoker(this);
            else return new ModuleActionInvoker(this);
        }

        /// <summary>
        /// Запечатан. Для определения дополнительной логики авторизации следует воспользоваться атрибутом <see cref="AuthorizeAttribute"/>.
        /// </summary>
        protected override void OnAuthorization(AuthorizationContext filterContext)
        { 
            var isAllowed = true;

            var moduleActionAttribute = (filterContext?.ActionDescriptor as ReflectedActionDescriptor)?.MethodInfo?.GetCustomAttributes<ModuleActionAttribute>(true).FirstOrDefault();
            if (moduleActionAttribute != null && moduleActionAttribute.Permission != Guid.Empty)
            {
                isAllowed = ModuleBase.CheckPermission(ModuleBase.GetAppCore().GetUserContextManager().GetCurrentUserContext(), moduleActionAttribute.Permission) == CheckPermissionVariant.Allowed;
            }

            if (!isAllowed)
            {
                var moduleAuth = AppCore.Get<OnXap.Modules.Auth.ModuleAuth>();
                moduleAuth.RememberUserContextRequestedAddressWhenRedirectedToAuthorization(filterContext.RequestContext.HttpContext.Request.Url);

                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                    {"controller", moduleAuth.UrlName },
                    {"action", "unauthorized" }, // todo заменить unauthorized на ссылку на метод. Но как, если возвращаемый результат ActionResult известен только при привязке к asp.net mvc/core?
                    {"area", AreaConstants.User}
                });
            }
        }

        /// <summary>
        /// Запечатан. Вместо этого метода следует пользоваться <see cref="OnBeforeExecution(ActionExecutingContext)"/>.
        /// </summary>
        protected sealed override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            AppCore.Get<OnXap.Modules.Routing.ModuleRouting>().PrepareCurrentThreadCache();

            if (filterContext.ActionDescriptor is ReflectedActionDescriptor)
            {
                var attr = (filterContext.ActionDescriptor as ReflectedActionDescriptor).MethodInfo.GetCustomAttributes(typeof(ModuleActionAttribute), true);
                if (attr == null) throw new InvalidOperationException("Поддерживается вызов только тех методов, к которым применен атрибут '" + typeof(ModuleActionAttribute).FullName + "'");
            }

            OnBeforeExecution(filterContext);
        }

        /// <summary>
        /// Запечатан. Вместо этого метода следует пользоваться <see cref="OnAfterExecution(ActionExecutedContext)"/>.
        /// </summary>
        protected sealed override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            filterContext.Result = PrepareActionResultToCurrentRequestType(filterContext.Result);
            OnAfterExecution(filterContext);
            AppCore.Get<OnXap.Modules.Routing.ModuleRouting>().ClearCurrentThreadCache();
        }

        /// <summary>
        /// Запечатан. Вместо этого метода следует пользоваться <see cref="ErrorHandled(Exception)"/>.
        /// Если <see cref="ErrorHandled(Exception)"/> возвращает null, то включается стандартное поведение <see cref="ControllerBase"/> с выбрасыванием стандартного окна ошибки. 
        /// </summary>
        protected sealed override void OnException(ExceptionContext filterContext)
        {
            try
            {
                var exception = filterContext.Exception;
                var message = exception.Message;
                if (exception is HttpCompileException d && !string.IsNullOrEmpty(d.SourceCode))
                {
                    var m = System.Text.RegularExpressions.Regex.Match(d.SourceCode, "#pragma checksum \"([^\"]+)\" \"\\{");
                    if (m.Success) message = $"Ошибка компиляции шаблона '{m.Groups[1].Value}'";
                }

                int code = 500;
                if (exception is ErrorCodeException exc)
                {
                    code = (int)exc.Code;
                    if (exception.InnerException != null) exception = exception.InnerException;
                }

                // Не регистрируем HttpRequestValidationException.
                if (!(exception is HttpRequestValidationException)) this.RegisterEventWithCode((HttpStatusCode)code, message, null, exception);

                // Отключаем реакцию на customErrors, делаем всегда кастомный вывод. Мб в будущем переделаем.
                //if (filterContext.HttpContext.IsCustomErrorEnabled)
                //{
                //    var result = this.ErrorHandled(exception);
                //    if (result != null)
                //    {
                //        filterContext.ExceptionHandled = true;
                //        filterContext.Result = result;
                //    }
                //    else base.OnException(filterContext);
                //}
                //else
                //{
                //    Debug.WriteLine("ModuleController.OnException: Внимание! Опция system.web/customErrors выставлена в Off, собственные шаблоны ошибок не используются!");
                //    base.OnException(filterContext);
                //}

                var result = this.ErrorHandled(exception);
                if (result != null)
                {
                    filterContext.ExceptionHandled = true;
                    filterContext.Result = result;
                }
                else base.OnException(filterContext);
            }
            catch (Exception ex2)
            {
                Debug.WriteLine("showError: " + ex2.Message);
                throw;
            }
        }

        /// <summary>
        /// Обрабатывает ошибку поиска метода в контроллере. 
        /// Вызывает <see cref="ErrorHandled(Exception)"/> с исключением <see cref="ErrorCodeException"/> с кодом 404.  
        /// </summary>
        /// <param name="actionName"></param>
        protected sealed override void HandleUnknownAction(string actionName)
        {
            Response.StatusCode = 404;

            this.RegisterEventWithCode(HttpStatusCode.NotFound, "Указанный адрес не найден", string.Format("Раздел '{0}' в модуле '{1}' отсутствует.", actionName, this.ModuleBase.Caption));

            var result = ErrorHandled(new ErrorCodeException(HttpStatusCode.NotFound, $"Раздел '{actionName}' в модуле '{this.ModuleBase.Caption}' отсутствует."));
            result = PrepareActionResultToCurrentRequestType(result);
            result.ExecuteResult(ControllerContext);
        }

        /// <summary>
        /// todo заполнить описание или избавиться от метода.
        /// </summary>
        protected ActionResult PrepareActionResultToCurrentRequestType(ActionResult result)
        {
            if (Types.RequestAnswerType.GetAnswerType() == Types.RequestAnswerType.Types.Json && !(result is JsonResult))
            {
                // result.e
                //    result = new JsonResult() {   }
            }

            return result;
        }

        #endregion

        #region Display
        /// <summary>
        /// </summary>
        [Obsolete("Такое присваивание переменной немного устарело. Переменная будет доступна в шаблоне через @ViewBag.varname, но более правильным будет все же использование модели.")]
        public void assignRef(string varname, object value)
        {
            assign(varname, value);
        }

        /// <summary>
        /// </summary>
        [Obsolete("Такое присваивание переменной немного устарело. Переменная будет доступна в шаблоне через @ViewBag.varname, но более правильным будет все же использование модели.")]
        public void assign(string varname, object value)
        {
            ViewData[varname] = value;
        }

        /// <summary>
        /// </summary>
        protected virtual void OnViewPrepare(object model)
        {

        }

        /// <summary>
        /// Позволяет выполнить дополнительные действия перед отображением представления. Например, передать дополнительные значения или изменить поведение.
        /// </summary>
        protected virtual void OnViewModule(object model)
        {

        }

        /// <summary>
        /// Обертка над <see cref="Controller.View(string, object)"/>
        /// </summary>
        [Obsolete("Рекомендуется View")]
        public ActionResult display(string template, object model = null)
        {
            return View(template, model);
        }

        /// <summary>
        /// Переопределяет поведение при поиске представлений.
        /// </summary>
        protected override ViewResult View(string viewName, string masterName, object model)
        {
            try
            {
                if (!string.IsNullOrEmpty(viewName))
                {
                    if (!viewName.EndsWith(".tpl") && !viewName.EndsWith(".cshtml") && !viewName.EndsWith(".vbhtml")) viewName += ".cshtml";
                    if (viewName.EndsWith(".tpl")) viewName = viewName.Replace(".tpl", ".cshtml");
                    if (!viewName.StartsWith("~/")) viewName = "~/" + viewName;
                }

                if (!string.IsNullOrEmpty(masterName))
                {
                    if (!masterName.EndsWith(".tpl") && !masterName.EndsWith(".cshtml") && !masterName.EndsWith(".vbhtml")) masterName += ".cshtml";
                    if (masterName.EndsWith(".tpl")) masterName = masterName.Replace(".tpl", ".cshtml");
                    if (!masterName.StartsWith("~/")) masterName = "~/" + masterName;
                }

                OnViewPrepare(model);
                return base.View(viewName, masterName, model);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }

        /**
        * Обертка над Smarty.display с дополнительными назначениями. Возвращает вывод шаблона в переменную
        * 
        * @param mixed $template
        */
        public string ViewString(string template, object model = null)
        {
            try
            {
                if (!template.EndsWith(".tpl") && !template.EndsWith(".cshtml") && !template.EndsWith(".vbhtml")) template += ".cshtml";
                if (template.EndsWith(".tpl")) template = template.Replace(".tpl", ".cshtml");
                if (!template.StartsWith("~/")) template = "~/" + template;

                OnViewPrepare(model);

                //ViewData.Model = model;
                using (var sw = new StringWriter())
                {
                    var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, template);
                    if (viewResult.View == null) throw new ArgumentException($"Представление '{template}' не найдено.", nameof(template));
                    ViewData.Model = model;
                    var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                    viewResult.View.Render(viewContext, sw);
                    viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                    return sw.GetStringBuilder().ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// Обертка над <see cref="Controller.Content(string)"/>.
        /// </summary>
        [Obsolete("Этот метод - просто имитация echo из PHP для более легкого перехода, по-правильному следует использовать родной метод Content")]
        public ContentResult Echo(string content)
        {
            return Content(content);
        }
        #endregion

        #region Json
        /// <summary>
        /// Создает шаблон ответа для <see cref="ReturnJson{TData}(Types.ResultWData{TData}, HttpStatusCode)"/>. 
        /// Им можно пользоваться во время выполнения функции и в конце передать в вызов <see cref="ReturnJson{TData}(Types.ResultWData{TData}, HttpStatusCode)"/>.
        /// </summary>
        /// <typeparam name="TData">См. <see cref="Types.ResultWData{TData}"/>.</typeparam>
        /// <returns></returns>
        public Types.ResultWData<TData> JsonAnswer<TData>()
        {
            return new Types.ResultWData<TData>() { ModelState = ModelState };
        }

        /// <summary>
        /// См. <see cref="JsonAnswer{TData}"/>.
        /// </summary>
        /// <returns></returns>
        public Types.ResultWData<object> JsonAnswer()
        {
            return JsonAnswer<object>();
        }

        /// <summary>
        /// Возвращает результат выполнения в стандартизированном json-формате TraceEngine.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="httpStatusCode">Статус http-ответа.</param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected internal JsonResult ReturnJson<TData>(Types.ResultWData<TData> result, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            return ReturnJsonInternal(result.Success, result.Message, result.Data, result.ModelState != null && !result.ModelState.IsValid ? result.ModelState : null, httpStatusCode);
        }

        /// <summary>
        /// Возвращает результат выполнения в стандартизированном json-формате TraceEngine.
        /// </summary>
        /// <param name="success">Результат выполнения скрипта - true или false.</param>
        /// <param name="message">Возвращаемый текст сообщения.</param>
        /// <param name="data">Дополнительные возвращаемые данные.</param>
        /// <param name="httpStatusCode">Статус http-ответа.</param>
        /// <returns></returns>
        protected internal JsonResult ReturnJson(bool success, string message, object data = null, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            return ReturnJsonInternal(success, message, data, ModelState, httpStatusCode);
        }

        private JsonResult ReturnJsonInternal(bool success, string message, object data, ModelStateDictionary modelState, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            object modelStateObject = null;
            if (modelState != null && !modelState.IsValid)
            {
                var d = modelState.ToErrorList();

                modelStateObject = d;

                if (d.Count > 0 && string.IsNullOrEmpty(message))
                {
                    var dd = d.SelectMany(x => x.Value).ToList();
                    message = "Во время проверки входящих данных обнаружены ошибки заполнения.\r\n - " + string.Join("\r\n - ", dd);
                }
            }

            Response.StatusCode = (int)httpStatusCode;
            return Json(new { success = success, result = string.IsNullOrEmpty(message) ? "" : message, data = data, modelState = modelStateObject }, JsonRequestBehavior.AllowGet);
        }

        public static Dictionary<string, string[]> GetErrors(ModelStateDictionary modelState, string prefix)
        {
            return modelState
                .Where(kvp => kvp.Value.Errors.Count > 0)
                .ToDictionary(kvp => String.IsNullOrWhiteSpace(kvp.Key) ? String.Empty : prefix == null ? kvp.Key : prefix.TrimEnd('.') + "." + kvp.Key,
                              kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());
        }

        protected sealed override JsonResult Json(object data, string contentType, Encoding contentEncoding)
        {
            return Json(data, contentType, contentEncoding, JsonRequestBehavior.AllowGet);
        }

        protected sealed override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new Types.NSJsonResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }
        #endregion

        #region Переадресация на другие модули.
        /// <summary>
        /// Выполняет переадресацию к методу в выражении <paramref name="expression"/> для контроллера <typeparamref name="TModuleController"/>.
        /// </summary>
        /// <seealso cref="RoutingManager.CreateRoute{TModule, TModuleController}(Expression{Func{TModuleController, ActionResult}}, bool)"/>
        public RedirectResult Redirect<TModule, TModuleController>(Expression<Func<TModuleController, ActionResult>> expression)
            where TModule : ModuleCore<TModule>
            where TModuleController : IModuleController<TModule>
        {
            return base.Redirect(AppCore.Get<RoutingManager>().CreateRoute<TModule, TModuleController>(expression, false)?.ToString());
        }
        #endregion

        #region Error
        /// <summary>
        /// Регистрирует событие в журнал HTTP-кодов.
        /// </summary>
        /// <param name="code">Код HTTP ошибки</param>
        /// <param name="message">См. <see cref="JournalingManager.RegisterEvent(int, EventType, string, string, DateTime?, Exception)"/>.</param>
        /// <param name="messageDetailed">См. <see cref="JournalingManager.RegisterEvent(int, EventType, string, string, DateTime?, Exception)"/>.</param>
        /// <param name="ex">См. <see cref="JournalingManager.RegisterEvent(int, EventType, string, string, DateTime?, Exception)"/>.</param>
        internal protected void RegisterEventWithCode(HttpStatusCode code, string message, string messageDetailed = null, Exception ex = null)
        {
            var idJournal = _journalsForErrors.GetOrAddWithExpiration((int)code, GetJournalForErrors, TimeSpan.FromMinutes(5));

            var msg = $"URL запроса: {Request.Url}\r\n";
            if (Request.UrlReferrer != null) msg += $"URL-referer: {Request.UrlReferrer}\r\n";

            var context = AppCore.GetUserContextManager().GetCurrentUserContext();

            if (context.IsGuest) msg += $"Пользователь: Гость\r\n";
            else msg += $"Пользователь: {context.GetData().ToString()} (id: {context.IdUser})\r\n";

            if (!string.IsNullOrEmpty(Request.UserAgent)) msg += $"User-agent: {Request.UserAgent}\r\n";
            var ipdns = new Dictionary<string, string>() { { "IP", Request.UserHostAddress }, { "DNS", Request.UserHostName } }.Where(x => !string.IsNullOrEmpty(x.Value)).ToList();
            if (ipdns.Count > 0) msg += $"User {string.Join(" / ", ipdns.Select(x => x.Key))}: {string.Join("/", ipdns.Select(x => x.Value))}\r\n";

            msg += messageDetailed;

            var errorType = (int)code == 500 ? EventType.CriticalError : EventType.Error;
            AppCore.Get<JournalingManager>().RegisterEvent(idJournal, errorType, message, msg, null, ex);
        }

        /// <summary>
        /// Для записи дополнительных данных в случае поиска ошибок в формах. Для внутреннего пользования.
        /// </summary>
        internal protected void RegisterEventInvalidModel(string message, string messageDetailed = null, Exception ex = null, IList<string> ignoreParamsKeys = null)
        {
            var msg = messageDetailed?.Trim();
            msg += "\r\n";
            if (Request.Form.Count > 0) msg += "POST: " + Newtonsoft.Json.JsonConvert.SerializeObject(Request.Form.AllKeys.Where(x => ignoreParamsKeys == null || !ignoreParamsKeys.Contains(x)).Select(x => new { key = x, values = Request.Form.GetValues(x) })) + "\r\n";

            RegisterEventWithCode(HttpStatusCode.NotAcceptable, message, msg, ex);
        }
        #endregion

        #region Свойства
        /// <summary>
        /// Для внутреннего использования. Some magic.
        /// </summary>
        internal virtual IModuleCore ModuleBase
        {
            get => throw new NotImplementedException();
        }

        /// <summary>
        /// Возвращает ядро приложения, в рамках которого запущен контроллер.
        /// </summary>
        public OnXApplication AppCore
        {
            get => GetAppCore();
        }

        /// <summary>
        /// Возвращает контекст доступа, ассоциированный с пользователем, от имени которого обрабатывается текущий запрос.
        /// </summary>
        public IUserContext UserContext
        {
            get => AppCore.GetUserContextManager().GetCurrentUserContext();
        }
        #endregion
    }

}
