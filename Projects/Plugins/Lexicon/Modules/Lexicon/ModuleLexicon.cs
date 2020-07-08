using OnUtils.Tasks;
using System;
using System.Collections.Generic;

namespace OnXap.Modules.Lexicon
{
    using Core.Modules;
    using TaskSheduling;

    /// <summary>
    /// Представляет модуль, предоставляющий функционал для работы с лексикой - склонение, формы слов и т.д.
    /// </summary>
    public abstract class ModuleLexicon : ModuleCore<ModuleLexicon>
    {
        private static ModuleLexicon _thisModule = null;

        protected override void OnModuleStarting()
        {
            _thisModule = this;

            var taskSchedulingManager = AppCore.Get<TaskSchedulingManager>();
            var task = taskSchedulingManager.RegisterTask(new TaskRequest()
            {
                Name = "Проверка новых необработанных слов",
                Description = "",
                IsEnabled = true,
                TaskOptions = TaskOptions.AllowDisabling | TaskOptions.AllowManualSchedule,
                UniqueKey = $"{typeof(LexiconManager).FullName}_{nameof(LexiconManager.PrepareNewWords)}",
                ExecutionLambda = () => LexiconNewWordsStatic()
            });
            if (task.ManualSchedules.Count == 0) taskSchedulingManager.SetTaskManualScheduleList(task, new List<TaskSchedule>() { new TaskCronSchedule(Cron.MinuteInterval(2)) { IsEnabled = true } });
        }

        #region Lexicon new words
        [ApiReversible]
        public static void LexiconNewWordsStatic()
        {
            _thisModule.AppCore.Get<LexiconManager>().PrepareNewWords();
        }
        #endregion

    }


}
