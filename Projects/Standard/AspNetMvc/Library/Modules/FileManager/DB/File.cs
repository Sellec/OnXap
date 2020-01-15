namespace OnXap.Modules.FileManager.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo ������ �����������.
    [Table("File")]
    public partial class File
    {
        [Key]
        public int IdFile { get; set; }

        public int IdModule { get; set; }

        [Required]
        [StringLength(260)]
        public string NameFile { get; set; }

        /// <summary>
        /// ���� � ����� ������������ �������� ����� �����.
        /// </summary>
        [Required]
        [StringLength(260)]
        public string PathFile { get; set; }

        public Guid? UniqueKey { get; set; }

        /// <summary>
        /// ����� ��� �����.
        /// </summary>
        public FileTypeCommon TypeCommon { get; set; }

        /// <summary>
        /// ������ mime-��� �����, ������������ �� ������ ����������, ���������� � ��.
        /// </summary>
        [StringLength(100)]
        public string TypeConcrete { get; set; }

        public int DateChange { get; set; }

        /// <summary>
        /// ���� ��������� ������� ����� �����, ����� ���� ���� ���������� �� �������� � ���������.
        /// </summary>
        public DateTime? DateExpire { get; set; }

        public int IdUserChange { get; set; }

        /// <summary>
        /// �������, ����������, ��� ���� ������� � ������� �� ��������.
        /// </summary>
        public bool IsRemoving { get; set; }

        /// <summary>
        /// �������, ����������, ��� ���� ������.
        /// </summary>
        public bool IsRemoved { get; set; }
    }
}
