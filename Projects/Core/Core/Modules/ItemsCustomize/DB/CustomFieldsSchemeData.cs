namespace OnXap.Modules.ItemsCustomize.DB
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo внести комментарии.
    [Table("CustomFieldsSchemeData")]
    public partial class CustomFieldsSchemeData
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdData { get; set; }

        [Key, Column(Order = 0)]
        public int IdModule { get; set; }

        [Key, Column(Order = 1)]
        public int IdItemType { get; set; }

        [Key, Column(Order = 2)]
        public int IdScheme { get; set; }

        [Key, Column(Order = 3)]
        public int IdSchemeItem { get; set; }

        [Key, Column(Order = 4)]
        public int IdField { get; set; }

        public int Order { get; set; }
    }
}
