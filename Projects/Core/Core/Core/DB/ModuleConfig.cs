using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnXap.Core.Db
{
    /// <summary>
    /// ��������� ��������� ������.
    /// </summary>
    [Table("ModuleConfig")]
    public class ModuleConfig
    {
        /// <summary>
        /// ������������� ������.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdModule { get; set; }

        /// <summary>
        /// ���������� ��������, ����������� ���������������� query-��� ������. ������������ ������ ��� query-����.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string UniqueKey { get; set; }

        /// <summary>
        /// ��������������� � json ��������� ������������ ������. ��. <see cref="Core.Configuration.ModuleConfiguration{TModule}"/>.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// ���� ���������� ��������� ������ � ����.
        /// </summary>
        public DateTime DateChange { get; set; }

        /// <summary>
        /// ������������� ������������, ��������� ��������� � ��������� ���.
        /// </summary>
        public int IdUserChange { get; set; }

    }
}
