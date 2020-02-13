using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo внести комментарии.
namespace OnXap.Modules.ItemsCustomize.DB
{
    [Table("CustomFieldsSchemeData")]
    public class CustomFieldsSchemeData
    {
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
