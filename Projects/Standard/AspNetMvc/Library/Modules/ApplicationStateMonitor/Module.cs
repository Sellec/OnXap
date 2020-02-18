using OnUtils.Tasks;
using System;
using System.Net;

namespace OnXap.Modules.ApplicationStateMonitor
{
    using Core.Modules;
    using Journaling;

    [ModuleCore("Монитор веб-сервера")]
    public class Module : ModuleCore<Module>
    {
        private static Module _this = null;

        protected override void OnModuleStarting()
        {
            _this = this;
            TasksManager.SetTask("WebMonitor", Cron.Minutely(), () => RunStatic());
        }

        protected override void OnModuleStop()
        {
            if (_this == this) _this = null;
        }

        private static void RunStatic()
        {
            var t = _this;
            if (t?.GetAppCore()?.GetState() != OnUtils.Architecture.AppCore.CoreComponentState.Started) return;

            try
            {
                var client = new WebClient();
                var defaultProxy = WebRequest.DefaultWebProxy;
                if (defaultProxy != null)
                {
                    defaultProxy.Credentials = CredentialCache.DefaultCredentials;
                    client.Proxy = defaultProxy;
                }
                var a = t.AppCore.ServerUrl;
                client.DownloadData(a);

                _this.RegisterEvent(EventType.Info, $"Активность IIS поддержана", null);
            }
            catch (WebException ex)
            {
                _this.RegisterEvent(EventType.Error, "Сетевая ошибка", null, ex);
                if (ex.Response == null)
                {
                    Debug.WriteLine($"{typeof(Module).FullName}.{nameof(RunStatic)}.1: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _this.RegisterEvent(EventType.Error, "Ошибка", null);
                Debug.WriteLine($"{typeof(Module).FullName}.{nameof(RunStatic)}.2: {ex.Message}");
            }
        }
    }
}