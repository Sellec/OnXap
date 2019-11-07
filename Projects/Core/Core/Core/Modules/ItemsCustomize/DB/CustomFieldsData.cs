namespace OnXap.Core.Modules.ItemsCustomize.DB
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo ������ �����������.
    [Table("CustomFieldsData")]
    public class CustomFieldsData
    {
        [Key]
        public long IdFieldData { get; set; }

        public int IdField { get; set; }

        public int IdItem { get; set; }

        public int IdItemType { get; set; }

        public int IdFieldValue { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string FieldValue { get; set; }

        public int DateChange { get; set; }
    }

}
