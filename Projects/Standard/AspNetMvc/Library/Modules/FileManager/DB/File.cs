using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo внести комментарии.
namespace OnXap.Modules.FileManager.Db
{
    [Table("File")]
    public class File
    {
        [Key]
        public int IdFile { get; set; }

        public int IdModule { get; set; }

        [Required]
        [StringLength(260)]
        public string NameFile { get; set; }

        /// <summary>
        /// Путь к файлу относительно корневой папки сайта.
        /// </summary>
        [Required]
        [StringLength(260)]
        public string PathFile { get; set; }

        public Guid? UniqueKey { get; set; }

        /// <summary>
        /// Общий тип файла.
        /// </summary>
        public FileTypeCommon TypeCommon { get; set; }

        /// <summary>
        /// Точный mime-тип файла, определенный на основе расширения, заголовков и пр.
        /// </summary>
        [StringLength(100)]
        public string TypeConcrete { get; set; }

        public int DateChange { get; set; }

        /// <summary>
        /// Дата истечения времени жизни файла, после чего файл помечается на удаление и удаляется.
        /// </summary>
        public DateTime? DateExpire { get; set; }

        public int IdUserChange { get; set; }

        /// <summary>
        /// Признак, означающий, что файл помещен в очередь на удаление.
        /// </summary>
        public bool IsRemoving { get; set; }

        /// <summary>
        /// Признак, означающий, что файл удален.
        /// </summary>
        public bool IsRemoved { get; set; }
    }
}
