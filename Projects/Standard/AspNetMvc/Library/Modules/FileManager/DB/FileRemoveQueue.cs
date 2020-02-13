using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Modules.FileManager.Db
{
    [Table("FileRemoveQueue")]
    class FileRemoveQueue
    {
        [Key]
        public int IdFile { get; set; }
    }
}
