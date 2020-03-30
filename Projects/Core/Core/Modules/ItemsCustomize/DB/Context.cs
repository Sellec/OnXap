using Microsoft.EntityFrameworkCore;

namespace OnXap.Modules.ItemsCustomize.DB
{
    using Core.Db;
    using Core.Items.Db;

#pragma warning disable CS1591 // todo внести комментарии.
    public class Context : CoreContextBase
    {
        public DbSet<CustomFieldsData> CustomFieldsDatas { get; set; }

        public DbSet<CustomFieldsField> CustomFieldsFields { get; set; }

        public DbSet<CustomFieldsScheme> CustomFieldsSchemes { get; set; }

        public DbSet<CustomFieldsSchemeData> CustomFieldsSchemeDatas { get; set; }

        public DbSet<CustomFieldsValue> CustomFieldsValues { get; set; }

        public DbSet<ItemParent> ItemParent { get; set; }

    }
}
