using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace OnXap.Core.Data
{
    using System.Collections.Generic;
    using MetadataObject;

    class DbStateManager : StateManager
    {
        public DbStateManager(StateManagerDependencies dependencies) : base(dependencies)
        {
        }

        public override InternalEntityEntry GetOrCreateEntry(object entity)
        {
            if (entity is MetadataObject metadataObject)
            {

            }
            return base.GetOrCreateEntry(entity);
        }

        public override InternalEntityEntry GetOrCreateEntry(object entity, IEntityType entityType)
        {
            if (entity is MetadataObject metadataObject)
            {

            }
            return base.GetOrCreateEntry(entity, entityType);
        }

        public override InternalEntityEntry TryGetEntry(IKey key, object[] keyValues)
        {
            var entry = base.TryGetEntry(key, keyValues);
            if (entry?.Entity is MetadataObject metadataObject)
            {

            }
            return entry;
        }

        public override InternalEntityEntry TryGetEntry(IKey key, object[] keyValues, bool throwOnNullKey, out bool hasNullKey)
        {
            var entry = base.TryGetEntry(key, keyValues, throwOnNullKey, out hasNullKey);
            if (entry?.Entity is MetadataObject metadataObject)
            {

            }
            return entry;
        }

        public override InternalEntityEntry TryGetEntry(object entity, bool throwOnNonUniqueness = true)
        {
            var entry = base.TryGetEntry(entity, throwOnNonUniqueness);
            if (entry?.Entity is MetadataObject metadataObject)
            {

            }
            return entry;
        }

        public override InternalEntityEntry TryGetEntry(object entity, IEntityType entityType, bool throwOnTypeMismatch = true)
        {
            var entry = base.TryGetEntry(entity, entityType, throwOnTypeMismatch);
            if (entry?.Entity is MetadataObject metadataObject)
            {

            }
            return entry;
        }
    }
}
