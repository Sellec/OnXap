using System;
using System.Collections.Generic;
using System.Linq;

namespace OnXap.Modules.Sitemap
{
    using Core;
    using Core.Items;
    using Core.Modules;
    using ServiceMonitor;
    using TaskSheduling;

    /// <summary>
    /// Модуль карты сайта.
    /// </summary>
    [ModuleCore("Карта сайта")]
    public class ModuleSitemap : ModuleCore<ModuleSitemap>
    {
        public const string PERM_SITEMAP = "sitemap";

        private TaskDescription _sitemapTask = null;

        /// <summary>
        /// </summary>
        protected override void OnModuleStarting()
        {
            RegisterPermission(PERM_SITEMAP, "Управление картой сайта");

            _sitemapTask = AppCore.Get<TaskSchedulingManager>().RegisterTask(new TaskRequest()
            {
                Name = $"Карта сайта: генерация",
                Description = "",
                IsEnabled = true,
                TaskOptions = TaskOptions.PreventParallelExecution | TaskOptions.AllowDisabling | TaskOptions.AllowManualSchedule,
                UniqueKey = GetType().FullName + "_Execute",
                ExecutionLambda = () => Execute(),
                Schedules = new List<TaskSchedule>() { new TaskCronSchedule(Cron.Daily(5)) }
            });
        }

        /// <summary>
        /// </summary>
        protected override void OnModuleStop()
        {
            _sitemapTask = null;
        }

        /// <summary>
        /// Инициирует немедленный запуск сервиса генерации карты сайта.
        /// </summary>
        public void MarkSitemapGenerationToRun()
        {
            if (_sitemapTask == null) throw new InvalidOperationException("Сервис генерации карты сайта недоступен. Возможно, модуль был некорректно инициализирован.");
            AppCore.Get<TaskSchedulingManager>().ExecuteTask(_sitemapTask);
        }

        private void Execute()
        {
            try
            {
                var sitemapProviderTypes = AppCore.GetQueryTypes().Where(x => typeof(ISitemapProvider).IsAssignableFrom(x)).ToList();
                var providerList = sitemapProviderTypes.Select(x =>
                {
                    try
                    {
                        return AppCore.Create<ISitemapProvider>(x);
                    }
                    catch
                    {
                        return null;
                    }
                }).Where(x => x != null).ToList();
                var linksAll = providerList.SelectMany(x => x.GetItems() ?? new List<SitemapItem>()).ToList();

                var module = AppCore.GetModulesManager().GetModule<ModuleSitemap>();

                var code = WebUtils.RazorRenderHelper.RenderView(module, "SitemapXml.cshtml", linksAll);

                var path = System.IO.Path.Combine(OnUtils.LibraryEnumeratorFactory.LibraryDirectory, "data/sitemap.xml");
                System.IO.File.WriteAllText(path, code);
            }
            catch(Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.CriticalError, "Ошибка построения карты сайта", null, ex);
            }
        }

    }
}