using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OnXap.Modules.Customer.Model
{
    using Core.DB;
    using Core.Items;
    using Modules.ItemsCustomize;
    using Modules.ItemsCustomize.Data;

    [ItemTypeAlias(typeof(User))]
    public class ProfileEdit : ItemBase, IItemCustomized
    {
        public ProfileEdit()
        {
        }

        public ProfileEdit(User source)
        {
            this.IdUser = source.IdUser;
            this.email = source.email;
            this.phone = source.phone;
            this.name = source.name;
            this.IdPhoto = source.IdPhoto;
            this.Comment = source.Comment;
        }

        [DefaultValue(0)]
        [Display(Name = "Идентификатор пользователя")]
        public int IdUser { get; set; }

        [DefaultValue("")]
        [StringLength(128)]
        [Display(Name = "Email-адрес"), DataType(DataType.EmailAddress), EmailAddress]
        public string email { get; set; }

        [DefaultValue("")]
        [StringLength(100)]
        [Display(Name = "Телефон"), DataType(DataType.PhoneNumber), PhoneFormat]
        public string phone { get; set; }

        [StringLength(100)]
        [Display(Name = "Имя")]
        public string name { get; set; }

        //[Required]
        [Display(Name = "Фотография")]
        [FileDataType(FileType.Image)]
        public int? IdPhoto { get; set; }

        [StringLength(2000)]
        [Display(Name = "Комментарий о себе"), DataType(DataType.MultilineText)]
        public string Comment { get; set; }

        public override int ID
        {
            get => IdUser;
        }

        public override string Caption
        {
            get;
        }

        public DefaultSchemeWData Fields => FieldsBase;

        public DefaultSchemeWData fields => fieldsBase;

        public dynamic FieldsDynamic => FieldsDynamicBase;

        public ReadOnlyDictionary<string, FieldData> fieldsNamed => fieldsNamedBase;
    }
}