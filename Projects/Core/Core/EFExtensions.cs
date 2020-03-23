using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EFExtensions
{
    public static class EFExtensions
    {
        public static EntityOp<TEntity> Upsert<TEntity>(this DbContext context, IEnumerable<TEntity> entity) where TEntity : class
        {
            return new UpsertOp<TEntity>(context, entity);
        }
    }

    public abstract class EntityOp<TEntity, TRet>
    {
        public readonly DbContext Context;
        public readonly IEnumerable<TEntity> EntityList;
        public readonly string TableName;
        public readonly string EntityPrimaryKeyName;

        private readonly List<string> keyNames = new List<string>();
        public IEnumerable<string> KeyNames { get => keyNames; }
        public IEnumerable<string> ExcludePropertiesFromUpdate { get => excludePropertiesFromUpdate; }

        private readonly List<string> excludeProperties = new List<string>();
        private readonly List<string> excludePropertiesFromUpdate = new List<string>();

        private static string GetMemberName<T>(Expression<Func<TEntity, T>> selectMemberLambda)
        {
            var member = selectMemberLambda.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException("The parameter selectMemberLambda must be a member accessing labda such as x => x.Id", "selectMemberLambda");
            }
            return member.Member.Name;
        }

        public EntityOp(DbContext context, IEnumerable<TEntity> entityList)
        {
            Context = context;
            EntityList = entityList;
            TableName = GetTableName(typeof(TEntity), context, out EntityPrimaryKeyName);
        }

        public abstract TRet Execute();
        public void Run()
        {
            Execute();
        }

        public EntityOp<TEntity, TRet> Key<TKey>(Expression<Func<TEntity, TKey>> selectKey)
        {
            keyNames.Add(GetMemberName(selectKey));
            return this;
        }

        public EntityOp<TEntity, TRet> ExcludeField<TField>(Expression<Func<TEntity, TField>> selectField)
        {
            excludeProperties.Add(GetMemberName(selectField));
            return this;
        }

        public EntityOp<TEntity, TRet> ExcludeFieldFromUpdate<TField>(Expression<Func<TEntity, TField>> selectField)
        {
            excludePropertiesFromUpdate.Add(GetMemberName(selectField));
            return this;
        }

        public Dictionary<PropertyInfo, string> GetColumnProperties()
        {
            // Dont include virtual navigation properties
            var propertiesList = typeof(TEntity).GetProperties().Where(pr => !excludeProperties.Contains(pr.Name) && !pr.GetMethod.IsVirtual).ToList();

            var mappings = GetTableMappings(typeof(TEntity), Context);
            var properties = propertiesList.Join(mappings.PropertyMappings,
                                                   property => property.Name,
                                                   mapping => mapping.Property.Name,
                                                   (property, mapping) => new { property, (mapping as ScalarPropertyMapping).Column }).ToDictionary(x => x.property, x => x.Column.Name);

            return properties;
        }

        public static string GetTableName(Type type, DbContext context, out string EntityPrimaryKeyName)
        {
            var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            // Get the entity type from the model that maps to the CLR type
            var entityType = metadata
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .Single(e => objectItemCollection.GetClrType(e) == type);

            // Get the entity set that uses this entity type
            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                .Single()
                .EntitySets
                .Single(s => s.ElementType.Name == entityType.Name);

            // Find the mapping between conceptual and storage model for this entity set
            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                    .Single()
                    .EntitySetMappings
                    .Single(s => s.EntitySet == entitySet);

            // Get the name of the primary key for the table as we wish to exclude this from the column mapping (we are assuming Identity insert is OFF)
            EntityPrimaryKeyName = mapping.EntitySet.ElementType.KeyMembers.
                Where(p => p.MetadataProperties.Any(m => m.PropertyKind == PropertyKind.Extended && Convert.ToString(m.Value) == "Identity")).
                Select(k => k.Name).
                FirstOrDefault();

            // Find the storage entity set (table) that the entity is mapped
            var table = mapping
                .EntityTypeMappings.Single()
                .Fragments.Single()
                .StoreEntitySet;

            // Return the table name from the storage entity set
            return (string)table.MetadataProperties["Table"].Value ?? table.Name;
        }

        public static MappingFragment GetTableMappings(Type type, DbContext context)
        {
            var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            // Get the entity type from the model that maps to the CLR type
            var entityType = metadata
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .Single(e => objectItemCollection.GetClrType(e) == type);

            // Get the entity set that uses this entity type
            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                .Single()
                .EntitySets
                .Single(s => s.ElementType.Name == entityType.Name);

            // Find the mapping between conceptual and storage model for this entity set
            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                    .Single()
                    .EntitySetMappings
                    .Single(s => s.EntitySet == entitySet);

            // Find the storage entity set (table) that the entity is mapped
            var tableMappings = mapping
                .EntityTypeMappings.Single()
                .Fragments.Single();

            return tableMappings;
        }
    }

    public abstract class EntityOp<TEntity> : EntityOp<TEntity, int>
    {
        public EntityOp(DbContext context, IEnumerable<TEntity> entityList) : base(context, entityList) { }

        public sealed override int Execute()
        {
            return ExecuteRet();
        }

        protected abstract int ExecuteRet();
    }

    public class UpsertOp<TEntity> : EntityOp<TEntity>
    {
        public UpsertOp(DbContext context, IEnumerable<TEntity> entityList) : base(context, entityList) { }

        protected override int ExecuteRet()
        {
            int countAffected = 0;
            var columnList = new List<string>();

            var columnProperties = GetColumnProperties();
            var columns = columnProperties.Values.ToArray();
            var updatableColumns = columnProperties.Where(x => !ExcludePropertiesFromUpdate.Contains(x.Key.Name) && x.Value != EntityPrimaryKeyName).Select(x => x.Value).ToArray();
            var insertableColumns = columns.Where(x => x != EntityPrimaryKeyName).ToArray();

            int batchSize = (int)Math.Truncate(2098.0 / columns.Length);

            for (int rows = 0; rows < EntityList.Count(); rows += batchSize)
            {
                var entities = EntityList.Skip(rows).Take(batchSize).ToList();
                if (entities.Count == 0) continue;

                var valueList = new List<object>();
                foreach (var entity in entities)
                {
                    foreach (var p in columnProperties.Keys)
                    {
                        var val = p.GetValue(entity, null);
                        valueList.Add(val ?? DBNull.Value);
                    }
                }

                StringBuilder sql = new StringBuilder();

                sql.Append("merge into ");
                sql.Append(TableName);
                sql.Append(" as T ");

                sql.Append("using (values (");

                for (int i = 0, valueKeyCounter = 0; i < entities.Count; i++)
                {
                    var valueKeyList = new List<string>();
                    for (int j = 0; j < columns.Length; j++, valueKeyCounter++)
                    {
                        valueKeyList.Add("{" + valueKeyCounter + "}");
                    }

                    sql.Append(string.Join(",", valueKeyList.ToArray()));
                    if (i < (entities.Count - 1)) sql.Append("), (");
                }

                sql.Append(")) as S (");
                sql.Append(string.Join(",", columns));
                sql.Append(") ");

                sql.Append("on (");
                var mergeCond = string.Join(" and ", KeyNames.Select(kn => "T." + kn + "=S." + kn));
                sql.Append(mergeCond);
                sql.Append(") ");

                sql.Append("when matched then update set ");
                sql.Append(string.Join(",", updatableColumns.Select(c => "T." + c + "=S." + c).ToArray()));

                sql.Append(" when not matched then insert (");
                sql.Append(string.Join(",", insertableColumns));
                sql.Append(") values (S.");
                sql.Append(string.Join(",S.", insertableColumns));
                sql.Append(");");
                var command = sql.ToString();

                countAffected += Context.Database.ExecuteSqlCommand(command, valueList.ToArray());
            }

            return countAffected;
        }
    }
}