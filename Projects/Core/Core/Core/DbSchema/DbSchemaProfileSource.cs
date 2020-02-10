using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OnXap.Core.DbSchema
{
    class DbSchemaProfileSource : IProfileSource
    {
        private readonly IFilteringMigrationSource _source;
        private readonly IMigrationRunnerConventions _conventions;

        public DbSchemaProfileSource(IFilteringMigrationSource source,IMigrationRunnerConventions conventions)
        {
            _source = source;
            _conventions = conventions;
        }

        public IEnumerable<IMigration> GetProfiles(string profile) => _source.GetMigrations(t => IsSelectedProfile(t, profile));

        private bool IsSelectedProfile(Type type, string profile)
        {
            if (typeof(DbSchemaItemRegular).IsAssignableFrom(type)) return true;

            if (!_conventions.TypeIsProfile(type)) return false;
            var profileAttribute = type.GetCustomAttribute<ProfileAttribute>(true);
            return !string.IsNullOrEmpty(profile) && string.Equals(profileAttribute.ProfileName, profile);
        }
    }
}
