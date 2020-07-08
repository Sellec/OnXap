using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnXap.Modules.Adminmain.Model
{
    public class TaskShedulingTask
    {
        public TaskShedulingTask()
        {
            ScheduleList = new List<TaskShedulingSchedule>();
            ManualScheduleList = new List<TaskShedulingSchedule>();
        }

        public TaskShedulingTask(TaskSheduling.TaskDescription taskDescription)
        {
            Id = taskDescription.Id;
            Name = taskDescription.Name;
            Description = taskDescription.Description;
            IsEnabled = taskDescription.IsEnabled;
            IsConfirmed = taskDescription.IsConfirmed;
            TaskOptions = taskDescription.TaskOptions;
            ScheduleList = taskDescription.Schedules.Select(x => new TaskShedulingSchedule(x)).ToList();
            ManualScheduleList = taskDescription.ManualSchedules.Select(x => new TaskShedulingSchedule(x)).ToList();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsConfirmed { get; set; }
        public TaskSheduling.TaskOptions TaskOptions { get; set; }
        public List<TaskShedulingSchedule> ScheduleList { get; set; }
        public List<TaskShedulingSchedule> ManualScheduleList { get; set; }
    }

    public enum TaskShedulingScheduleType : int
    {
        Cron = 1,
        DateTimeFixed = 2
    }

    public class TaskShedulingSchedule
    {
        public TaskShedulingSchedule()
        {
        }

        public TaskShedulingSchedule(TaskSheduling.TaskSchedule taskSchedule)
        {
            IsEnabled = taskSchedule.IsEnabled;
            if (taskSchedule is TaskSheduling.TaskCronSchedule taskCronSchedule)
            {
                Cron = taskCronSchedule.CronExpression;
                Type = TaskShedulingScheduleType.Cron;
            }
            else if (taskSchedule is TaskSheduling.TaskFixedTimeSchedule taskFixedTimeSchedule)
            {
                DateTimeFixed = taskFixedTimeSchedule.DateTime;
                Type = TaskShedulingScheduleType.DateTimeFixed;
            }
        }

        public TaskShedulingScheduleType Type { get; set; }
        public string Cron { get; set; }
        public DateTimeOffset? DateTimeFixed { get; set; }
        public bool IsEnabled { get; set; }
    }
}