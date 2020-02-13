using System;
using System.Collections.Generic;
using System.Text;

namespace OnXap.Modules.Lexicon.Db
{
    using Core.DbSchema;

    class WordCaseSchemeItem: DbSchemaItemRegular
    {
        public override void Down()
        {

        }

        public override void Up()
        {
            if (!Schema.Table<WordCase>().Exists())
            {
                Create.Table<WordCase>().
                    WithColumn((WordCase x) => x.NominativeSingle).AsString(100).NotNullable().WithDefaultValue("").PrimaryKey().
                    WithColumn((WordCase x) => x.NominativeTwo).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.NominativeMultiple).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.GenitiveSingle).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.GenitiveTwo).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.GenitiveMultiple).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.DativeSingle).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.DativeTwo).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.DativeMultiple).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.AccusativeSingle).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.AccusativeTwo).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.AccusativeMultiple).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.InstrumentalSingle).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.InstrumentalTwo).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.InstrumentalMultiple).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.PrepositionalSingle).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.PrepositionalTwo).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.PrepositionalMultiple).AsString(100).NotNullable().WithDefaultValue("").
                    WithColumn((WordCase x) => x.IsNewSingle).AsBoolean().NotNullable().WithDefaultValue(true).
                    WithColumn((WordCase x) => x.IsNewTwo).AsBoolean().NotNullable().WithDefaultValue(true).
                    WithColumn((WordCase x) => x.IsNewMultiple).AsBoolean().NotNullable().WithDefaultValue(true);
            }
            else
            {
                AddColumnIfNotExists(Schema, (WordCase x) => x.NominativeSingle, x => x.AsString(100).NotNullable().WithDefaultValue("").PrimaryKey());
                AddColumnIfNotExists(Schema, (WordCase x) => x.NominativeTwo, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.NominativeMultiple, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.GenitiveSingle, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.GenitiveTwo, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.GenitiveMultiple, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.DativeSingle, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.DativeTwo, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.DativeMultiple, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.AccusativeSingle, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.AccusativeTwo, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.AccusativeMultiple, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.InstrumentalSingle, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.InstrumentalTwo, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.InstrumentalMultiple, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.PrepositionalSingle, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.PrepositionalTwo, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.PrepositionalMultiple, x => x.AsString(100).NotNullable().WithDefaultValue(""));
                AddColumnIfNotExists(Schema, (WordCase x) => x.IsNewSingle, x => x.AsBoolean().NotNullable().WithDefaultValue(true));
                AddColumnIfNotExists(Schema, (WordCase x) => x.IsNewTwo, x => x.AsBoolean().NotNullable().WithDefaultValue(true));
                AddColumnIfNotExists(Schema, (WordCase x) => x.IsNewMultiple, x => x.AsBoolean().NotNullable().WithDefaultValue(true));
            }
        }
    }
}
