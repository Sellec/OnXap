using OnXap.Core.Modules;
using OnXap.Modules.ItemsCustomize;
using OnXap.Modules.ItemsCustomize.Data;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // todo ������ �����������.
namespace OnXap.Core.Db
{
    public enum UserState : short
    {
        [Display(Name = "�������")]
        Active = 0,

        [Display(Name = "����������� ������� ������������� �� Email")]
        RegisterNeedConfirmation = 1,

        [Display(Name = "����������� ������� �������������")]
        RegisterWaitForModerate = 2,

        [Display(Name = "����������� ���������")]
        RegisterDecline = 3,

        [Display(Name = "���������")]
        Disabled = 4,
    }

    [Table("users")]
    [DisplayName("������������")]
    [System.Diagnostics.DebuggerDisplay("User: id={ID}")]
    public class User : UserBase, IItemCustomized
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int IdUser { get; set; }

        /// <summary>
        /// ��. <see cref="IdUser"/>.
        /// </summary>
        public override int ID => IdUser;

        /// <summary>
        /// ��. <see cref="UserBase.name"/>. 
        /// </summary>
        public override string Caption => !string.IsNullOrEmpty(name) ? name : !string.IsNullOrEmpty(email) ? email : IdUser.ToString();

        /// <summary>
        /// ����� ���������� ��������� �� ������ <see cref="UserBase.DateChange"/>. 
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
