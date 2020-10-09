using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.TaskSheduling.Db
{
    [Table("Task")]
    [DisplayName("Запускаемая задача")]
    class Task
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(int.MaxValue)]
        public string Description { get; set; }

        [Required, MaxLength(300)]
        public string UniqueKey { get; set; }

        public bool? IsEnabled { get; set; }

        public List<TaskSchedule> TaskSchedules { get; set; }
    }
}
