using FluentMigrator;

namespace OnXap.Core.DbSchema
{
    [Profile(DbSchemaDefaultProfile.ProfileName)]
    class DbSchemaDefaultProfile : DbSchemaItem
    {
        public const string ProfileName = "DbSchemaManager";

        public override void Down()
        {
        }

        public override void Up()
        {
        }
    }
}
