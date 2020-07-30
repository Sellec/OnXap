using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.TaskSheduling.Db
{
    /// <summary>
    /// Правила, созданные в дополнение к правилам, заданным при регистрации задачи.
    /// </summary>
    [Table("TaskSchedule")]
    class TaskSchedule
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int IdTask { get; set; }

        [MaxLength(200)]
        public string Cron { get; set; }

        public DateTime? DateTimeFixed { get; set; }

        public bool IsEnabled { get; set; }

        [ForeignKey(nameof(IdTask))]
        public Task Task { get; set; }

        public string GetUniqueKey()
        {
            return DateTimeFixed.HasValue ? DateTimeFixed.Value.ToString("yyyy-MM-dd HH:mm") : Cron;
        }
    }
}
