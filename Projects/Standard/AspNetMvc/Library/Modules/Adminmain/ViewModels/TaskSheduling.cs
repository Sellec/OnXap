using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnXap.Modules.Adminmain.ViewModels
{
    using OnXap.TaskSheduling;

    /// <summary>
    /// Модель для представления TaskSheduling.cshtml
    /// </summary>
    public class TaskSheduling
    {
        /// <summary>
        /// Список задач.
        /// </summary>
        public List<Model.TaskShedulingTask> TaskList { get; set; }
    }

}