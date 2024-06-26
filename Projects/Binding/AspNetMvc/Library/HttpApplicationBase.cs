﻿using OnUtils.Architecture.AppCore;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace OnXap
{
    using Core.Data;
    using Binding.Binders;
    using Binding.Providers;

    /// <summary>
    /// Представляет приложение ASP.NET, умеющее инициализировать OnXap.
    /// </summary>
    public abstract class HttpApplicationBase : HttpApplication
    {
        private static object SyncRootApplication = new object();
        private static volatile int _instancesCount = 0;
        private static OnXApplicationAspNetMvc _applicationCore = null;
        private static bool _applicationCoreStarted = false;
        private static Uri _urlFirst = null;
        internal static ApplicationRuntimeOptions _runtimeOptions;

        [ThreadStatic]
        internal Queue<IDisposable> _requestSpecificDisposables;

        /// <summary>
        /// Создает новый экземпляр приложения ASP.NET.
        /// </summary>
        protected HttpApplicationBase() : this(ApplicationRuntimeOptions.None)
        {
        }

        /// <summary>
        /// Создает новый экземпляр приложения ASP.NET.
        /// </summary>
        protected HttpApplicationBase(ApplicationRuntimeOptions runtimeOptions)
        {
            _runtimeOptions = runtimeOptions;
        }

        #region "Virtual"
        /// <summary>
        /// Вызывается во время запуска приложения до создания ядра приложения.
        /// </summary>
        protected virtual void OnBeforeApplicationStart()
        {
        }

        /// <summary>
        /// Вызывается во время запуска приложения после создания и инициализации ядра приложения.
        /// </summary>
        protected virtual void OnAfterApplicationStart()
        {
        }

        /// <summary>
        /// Вызывается во время начала обработки входящего запроса.
        /// </summary>
        protected virtual void OnBeginRequest()
        {
        }

        /// <summary>
        /// Вызывается после обработки входящего запроса.
        /// </summary>
        protected virtual void OnEndRequest()
        {
        }

        /// <summary>
        /// Вызывается при возникновении необработанной ошибки в приложении.
        /// </summary>
        protected virtual void OnError(Exception ex)
        {
        }

        /// <summary>
        /// Вызывается при остановке приложения до остановки ядра приложения.
        /// </summary>
        protected virtual void OnApplicationStopping()
        {

        }

        /// <summary>
        /// Вызывается при остановке приложения после остановки ядра приложения.
        /// </summary>
        protected virtual void OnApplicationStopped()
        {

        }

        /// <summary>
        /// Вызывается при запуске экземпляра HttpApplication.
        /// </summary>
        protected virtual void OnApplicationInstanceStarted()
        {

        }

        /// <summary>
        /// Вызывается при остановке экземпляра HttpApplication.
        /// </summary>
        protected virtual void OnApplicationInstanceStopped()
        {

        }

        /// <summary>
        /// Возвращает настройки подключения к базе.
        /// </summary>
        protected abstract IDbConfigurationBuilder GetDbConfigurationBuilder();

        #endregion

        #region HttpApplication
        internal void Application_Start()
        {
            HtmlHelper.ClientValidationEnabled = true;

            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            GlobalFilters.Filters.Add(new External.ActionParameterAlias.ParameterAliasAttributeGlobal());
            GlobalFilters.Filters.Add(new WebUtils.CompressBehaviourFilter());

            ModelBinders.Binders.Add(typeof(JsonDictionary), new JsonDictionaryModelBinder());
            ModelBinders.Binders.DefaultBinder = new TraceModelBinder();

            lock (SyncRootApplication)
            {
                if (_applicationCore == null)
                {
                    OnBeforeApplicationStart();

                    var physicalApplicationPath = Server.MapPath("~");

                    _applicationCore = new OnXApplicationAspNetMvc(physicalApplicationPath, GetDbConfigurationBuilder());
                    switch (_runtimeOptions)
                    {
                        case ApplicationRuntimeOptions.DebugLevelDetailed:
                            _applicationCore.AppDebugLevel = DebugLevel.Detailed;
                            break;

                        case ApplicationRuntimeOptions.DebugLevelCommon:
                            _applicationCore.AppDebugLevel = DebugLevel.Common;
                            break;
                    }
                    _applicationCoreStarted = false;
                }
            }

            try
            {
                OnApplicationInstanceStarted();
            }
            catch { }
        }

        /// <summary>
        /// Не убирать. Нужен для обмана Readonly-режима сессий, чтобы новые сессии создавались и записывались. Иначе будут меняться только существующие сессии.
        /// </summary>
        public void Session_OnStart()
        {
        }

        internal void Application_Error(Object sender, EventArgs e)
        {
            try
            {
                var exception = Server.GetLastError();
                // todo _applicationCore.OnError(exception);

                if (exception is HttpRequestValidationException)
                {
                    Response.Clear();
                    Response.StatusCode = 200;
                    Response.Write(@"[html]");
                    Response.End();
                }

                this.OnError(exception);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        internal void Application_BeginRequest(object sender, EventArgs e)
        {
            Context.Items["TimeRequestStart"] = DateTime.Now;

            var isFirstRequest = (bool?)Context.GetType().GetProperty("FirstRequest", BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic)?.GetValue(Context);
            if (isFirstRequest.HasValue && isFirstRequest.Value) _urlFirst = Request.Url;

            lock (SyncRootApplication)
            {
                if (_applicationCore == null)
                {
                    try
                    {
                        OnBeforeApplicationStart();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("OnApplicationStart: {0}", ex.Message);
                        throw;
                    }

                    var physicalApplicationPath = Server.MapPath("~");

                    _applicationCore = new OnXApplicationAspNetMvc(physicalApplicationPath, GetDbConfigurationBuilder());
                    switch (_runtimeOptions)
                    {
                        case ApplicationRuntimeOptions.DebugLevelDetailed:
                            _applicationCore.AppDebugLevel = DebugLevel.Detailed;
                            break;

                        case ApplicationRuntimeOptions.DebugLevelCommon:
                            _applicationCore.AppDebugLevel = DebugLevel.Common;
                            break;
                    }
                    _applicationCoreStarted = false;
                }

                if (!_applicationCore.IsServerUrlHasBeenSet && _urlFirst != null)
                    _applicationCore.ServerUrl = new UriBuilder(_urlFirst.Scheme, _urlFirst.Host, _urlFirst.Port).Uri;

                if (!_applicationCoreStarted)
                {
                    _applicationCore.Start();

                    try
                    {
                        OnAfterApplicationStart();
                        // todo     _applicationCore.OnApplicationAfterStartAfterUserEvent();
                        _applicationCoreStarted = true;
                    }
                    catch (Exception ex)
                    {
                        _applicationCore = null;
                        Debug.WriteLine("OnAfterApplicationStart: {0}", ex.Message);
                        throw;
                    }
                }
            }

            Core.Data.Helpers.QueryLogHelper.QueryLogEnabled = true;
            Context.Items["TimeRequestStart"] = DateTime.Now;

            HttpContext.Current.SetAppCore(_applicationCore);
            _applicationCore.GetUserContextManager().ClearCurrentUserContext();

            _requestSpecificDisposables = new Queue<IDisposable>();

            try
            {
                /*
                 * Попытка распарсить json из запроса в <see cref="Request.Form"/>.
                 * */

                if (Request.ContentType.IndexOf("application/json", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    if (Request.InputStream.CanRead)
                    {
                        try
                        {
                            if (Request.InputStream.CanSeek) Request.InputStream.Seek(0, SeekOrigin.Begin);
                            var body = Request.InputStream;
                            var encoding = Request.ContentEncoding;
                            var reader = new System.IO.StreamReader(body, encoding);
                            string s = reader.ReadToEnd();

                            var jsonRequestObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(s, new Newtonsoft.Json.JsonSerializerSettings()
                            {
                                Error = null,
                            });
                            if (jsonRequestObject != null && jsonRequestObject.Count > 0)
                            {
                                var oQuery = Request.Form;
                                oQuery = (NameValueCollection)Request.GetType().GetField("_form", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Request);
                                if (oQuery != null)
                                {
                                    var oReadable = oQuery.GetType().GetProperty("IsReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
                                    oReadable.SetValue(oQuery, false, null);
                                    foreach (var p in jsonRequestObject) Request.Form[p.Key] = p.Value?.ToString();
                                    oReadable.SetValue(oQuery, true, null);
                                }
                            }
                        }
                        finally { if (Request.InputStream.CanSeek) Request.InputStream.Seek(0, SeekOrigin.Begin); }
                    }
                }
            }
            catch (ThreadAbortException) { throw; }
            catch { }

            try
            {
                this.OnBeginRequest();
            }
            catch (ThreadAbortException) { throw; }
            catch (Exception ex) { Debug.WriteLine("OnBeginRequest: " + ex.Message); }
        }

        internal void Application_AcquireRequestState(object sender, EventArgs e)
        {
            Context.Items["TimeRequestState"] = DateTime.Now;
            if (_applicationCore.GetState() == CoreComponentState.Started)
            {
                var context = _applicationCore.Get<SessionBinder>().RestoreUserContextFromRequest();
                if (context != null) _applicationCore.GetUserContextManager().SetCurrentUserContext(context);
            }
        }

        internal void Application_EndRequest(Object sender, EventArgs e)
        {
            Context.Items["TimeRequestEnd"] = DateTime.Now;

            try
            {
                OnEndRequest();
            }
            catch (ThreadAbortException) { throw; }
            catch (Exception ex) { Debug.WriteLine("OnBeginRequest: " + ex.Message); }

            var requestSpecificDisposables = _requestSpecificDisposables;
            if (requestSpecificDisposables != null)
            {
                while (requestSpecificDisposables.Count > 0)
                {
                    var item = requestSpecificDisposables.Dequeue();
                    try
                    {
                        item.Dispose();
                    }
                    catch (Exception ex) { Debug.WriteLine("TraceHttpApplication.EndRequest Disposables: {0}", ex.Message); }
                }
            }
            _requestSpecificDisposables = null;

            TraceSessionStateProvider.SaveUnsavedSessionItem();

            if (_applicationCore.GetState() == CoreComponentState.Started)
            {
                _applicationCore.GetUserContextManager().ClearCurrentUserContext();
            }

            Core.Data.Helpers.QueryLogHelper.QueryLogEnabled = false;
            Core.Data.Helpers.QueryLogHelper.ClearQueries();
        }

        internal void Application_PreSendRequestHeaders()
        {
            HttpResponse response = HttpContext.Current.Response;
            if (response.Filter is GZipStream && response.Headers["Content-encoding"] != "gzip")
                response.AppendHeader("Content-encoding", "gzip");
            else if (response.Filter is DeflateStream && response.Headers["Content-encoding"] != "deflate")
                response.AppendHeader("Content-encoding", "deflate");
        }

        internal void Application_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            UpdateSessionCookieExpiration();
        }

        internal void Application_Disposed(Object sender, EventArgs e)
        {
            lock (SyncRootApplication)
            {
                if (_applicationCore == null)
                    return;

                try
                {
                    OnApplicationInstanceStopped();
                }
                catch { }
            }
        }

        internal void Application_End(Object sender, EventArgs e)
        {
            lock (SyncRootApplication)
            {
                if (_applicationCore == null)
                    return;

                try { OnApplicationStopping(); } catch { }

                try
                {
                    var appCore = _applicationCore;
                    _applicationCore = null;
                    if (appCore?.GetState() == CoreComponentState.Started) appCore.Stop();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error stopping application core: {ex.ToString()}");
                }

                try { OnApplicationStopped(); } catch { }
            }
        }
        #endregion

        private void UpdateSessionCookieExpiration()
        {
            var httpContext = HttpContext.Current;
            var sessionState = httpContext?.Session;

            if (sessionState == null) return;

            var sessionStateSection = System.Configuration.ConfigurationManager.GetSection("system.web/sessionState") as System.Web.Configuration.SessionStateSection;
            var sessionCookie = httpContext.Response.Cookies[sessionStateSection?.CookieName ?? "ASP.NET_SessionId"];

            if (sessionCookie == null) return;

            sessionCookie.Expires = DateTime.Now.AddMinutes(sessionState.Timeout);
            sessionCookie.HttpOnly = true;
            sessionCookie.Value = sessionState.SessionID;
        }

        /// <summary>
        /// </summary>
        public sealed override void Init()
        {
            _instancesCount++;
            base.Init();
        }

        /// <summary>
        /// </summary>
        public sealed override void Dispose()
        {
            _instancesCount--;
            base.Dispose();
        }

        #region Свойства
        /// <summary>
        /// Возвращает количество экземпляров приложения, запущенных в данный момент.
        /// </summary>
        public static int InstancesCount
        {
            get => _instancesCount;
        }

        /// <summary>
        /// Возвращает ядро приложения.
        /// </summary>
        public OnXApplication AppCore
        {
            get => _applicationCore;
        }

        #endregion

    }
}
