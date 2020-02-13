using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Modules.FileManager.Db
{
    [Table("File")]
    class FileCountUsage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdFile { get; set; }

        public int CountUsage { get; set; }
    }
}
