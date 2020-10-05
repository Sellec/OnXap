using NCrontab;
using System;

namespace OnXap.TaskSheduling
{
    class JobInternal
    {
        public string JobName { get; set; }

        public CrontabSchedule CronSchedule { get; set; }

        public Action ExecutionDelegate { get; set; }

        public DateTime ClosestOccurrence { get; set; }
    }
}
