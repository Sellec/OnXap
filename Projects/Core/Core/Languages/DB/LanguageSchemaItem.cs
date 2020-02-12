namespace OnXap.Languages.DB
{
    using Core.DbSchema;

    class LanguageSchemaItem : DbSchemaItemRegular
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            if (!Schema.Table<Language>().Exists())
            {
                Create.Table<Language>().
                    WithColumn((Language x) => x.IdLanguage).AsInt32().NotNullable().PrimaryKey().Identity().
                    WithColumn((Language x) => x.NameLanguage).AsString(200).NotNullable().
                    WithColumn((Language x) => x.ShortAlias).AsString(20).NotNullable().
                    WithColumn((Language x) => x.IsDefault).AsInt32().NotNullable().
                    WithColumn((Language x) => x.TemplatesPath).AsString(200).NotNullable().
                    WithColumn((Language x) => x.Culture).AsString(20).NotNullable().WithDefaultValue("");
            }
            else
            {
                AddColumnIfNotExists(Schema, (Language x) => x.IdLanguage, x => x.AsInt32().NotNullable().PrimaryKey().Identity());
                AddColumnIfNotExists(Schema, (Language x) => x.NameLanguage, x => x.AsString(200).NotNullable());
                AddColumnIfNotExists(Schema, (Language x) => x.ShortAlias, x => x.AsString(20).NotNullable());
                AddColumnIfNotExists(Schema, (Language x) => x.TemplatesPath, x => x.AsString(200).NotNullable());
                AddColumnIfNotExists(Schema, (Language x) => x.TemplatesPath, x => x.AsInt32().NotNullable());
                AddColumnIfNotExists(Schema, (Language x) => x.Culture, x => x.AsString(20).NotNullable().WithDefaultValue(""));
            }
        }
    }
}
