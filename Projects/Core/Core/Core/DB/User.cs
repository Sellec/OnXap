using OnXap.Core.Modules;
using OnXap.Modules.ItemsCustomize;
using OnXap.Modules.ItemsCustomize.Data;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo внести комментарии.
namespace OnXap.Core.Db
{
    public enum UserState : short
    {
        [Display(Name = "Активна")]
        Active = 0,

        [Display(Name = "Регистрация ожидает подтверждения по Email")]
        RegisterNeedConfirmation = 1,

        [Display(Name = "Регистрация ожидает модерирования")]
        RegisterWaitForModerate = 2,

        [Display(Name = "Регистрация отклонена")]
        RegisterDecline = 3,

        [Display(Name = "Отключена")]
        Disabled = 4,
    }

    [Table("users")]
    [DisplayName("Пользователь")]
    [System.Diagnostics.DebuggerDisplay("User: id={ID}")]
    public class User : UserBase, IItemCustomized
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int IdUser { get; set; }

        /// <summary>
        /// См. <see cref="IdUser"/>.
        /// </summary>
        public override int ID => IdUser;

        /// <summary>
        /// См. <see cref="UserBase.name"/>. 
        /// </summary>
        public override string Caption => !string.IsNullOrEmpty(name) ? name : !string.IsNullOrEmpty(email) ? email : IdUser.ToString();

        /// <summary>
        /// Время последнего изменения на основе <see cref="UserBase.DateChange"/>. 
        /// </summary>
        public override DateTime DateChangeBase
        {
            get => DateChange.FromTimestamp();
            set => DateChange = value.Timestamp();
        }

        public override ModuleCore OwnerModule => base.OwnerModule;

        public DefaultSchemeWData Fields => FieldsBase;

        public DefaultSchemeWData fields => fieldsBase;

        public dynamic FieldsDynamic => FieldsDynamicBase;

        public ReadOnlyDictionary<string, FieldData> fieldsNamed => fieldsNamedBase;
    }
}
