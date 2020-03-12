using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OnUtils.Items;
using OnUtils.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace OnXap.Core.Data
{
    /// <summary>
    /// Расширенные возможности стандартного <see cref="DbContext"/>.
    /// </summary>
    public abstract class DbContextBase : DbContext
    {
        internal DbContextBase()
        {
        }

        #region OnConfiguring
        /// <summary>
        /// </summary>
        protected sealed override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            OnConfiguringInternal(optionsBuilder);
        }

        internal virtual void OnConfiguringInternal(DbContextOptionsBuilder optionsBuilder)
        {
        }
        #endregion

        #region OnModelCreating
        /// <summary>
        /// </summary>
        protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityTypeFirst in modelBuilder.Model.GetEntityTypes())
            {
                if (entityTypeFirst is EntityType entityType)
                {
                    if (entityType.HasClrType())
                    {
                        Dapper.SqlMapper.SetTypeMap(entityType.ClrType, Activator.CreateInstance(typeof(DapperColumnAttributeTypeMapper<>).MakeGenericType(entityType.ClrType)) as SqlMapper.ITypeMap);
                    }

                    if (entityType.BaseType == null)
                    {
                        var primaryKey = entityType.FindPrimaryKey();
                        if ((primaryKey != null ? (primaryKey.Properties.Count > 1 ? 1 : 0) : 0) != 0)
                        {
                            if (entityType.GetPrimaryKeyConfigurationSource() == ConfigurationSource.DataAnnotation)
                            {
                                var keyProperties = entityType.GetProperties().Where(x => x.IsKey() && x.GetConfigurationSource() != ConfigurationSource.Convention).ToList();
                                entityType.SetPrimaryKey(keyProperties, ConfigurationSource.Explicit);
                            }
                        }
                    }
                    else
                    {
                        foreach (Property declaredProperty in entityType.GetDeclaredProperties())
                        {
                            MemberInfo identifyingMemberInfo = declaredProperty.GetIdentifyingMemberInfo();
                            if (identifyingMemberInfo != (MemberInfo)null && Attribute.IsDefined(identifyingMemberInfo, typeof(KeyAttribute), true))
                            {
                                throw new NotImplementedException("EF Core hack with replacing data annotation primary keys - not implemented fully.");
                            }
                        }
                    }
                }
            }

            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                var decimalAttribute = property.PropertyInfo.GetCustomAttribute<DecimalPrecisionAttribute>(true);
                if (decimalAttribute != null) property.SetColumnType($"decimal({decimalAttribute.Precision}, {decimalAttribute.Scale})");
            }

            OnModelCreatingInternal(modelBuilder);
        }

        internal virtual void OnModelCreatingInternal(ModelBuilder modelBuilder)
        {
        }
        #endregion

        #region Работа с объектами
        /// <summary>
        /// Возвращает результат выполнения запроса внутри контекста. В качестве запроса может выступать строка, объект и т.п. - зависит от возможностей конкретной реализации контекста.
        /// Результат выполнения запроса возвращается в виде перечисления объектов типа <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="query">Запрос, который необходимо выполнить.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам запроса.
        /// Это может быть анонимный тип, например, для запроса с условием "DateChange=@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        /// <param name="cacheInLocal">Указывает, следует ли кешировать объекты, созданные в результате выполнения запроса, во внутренних репозиториях соответствующих типов.</param>
        /// <param name="entityExample"></param>
        public IEnumerable<TEntity> ExecuteQuery<TEntity>(object query, object parameters = null, bool cacheInLocal = false, TEntity entityExample = default(TEntity)) 
            where TEntity : class
        {
            try
            {
                var queryText = query.ToString();

                if (TypeHelper.IsAnonymousType(typeof(TEntity)))
                {
                    var type = typeof(TEntity);
                    var properties = type.GetProperties().ToDictionary(x => x, x => type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(y => y.Name.StartsWith("<" + x.Name + ">")).FirstOrDefault());

                    var t = typeof(Dapper.SqlMapper).GetNestedType("DapperRow", BindingFlags.NonPublic);
                    if (t != null)
                    {
                        var dapperTypeContainsKey = t.GetMethod("System.Collections.Generic.IDictionary<System.String,System.Object>.ContainsKey", BindingFlags.NonPublic | BindingFlags.Instance);
                        var dapperTypeGet = t.GetMethod("System.Collections.Generic.IDictionary<System.String,System.Object>.get_Item", BindingFlags.NonPublic | BindingFlags.Instance);

                        if (dapperTypeContainsKey != null && dapperTypeGet != null)
                        {
                            var dynamicParameters = new DynamicParameters(parameters);
                            var results = Database.GetDbConnection().Query(queryText, dynamicParameters, buffered: cacheInLocal, commandTimeout: Database.GetCommandTimeout());

                            return results.Select(res =>
                            {
                                var obj = (TEntity)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(TEntity));
                                properties.Where(x => x.Value != null).ForEach(x => x.Value.SetValue(obj, dapperTypeContainsKey.Invoke(res, new object[] { x.Key.Name }).Equals(true) ? dapperTypeGet.Invoke(res, new object[] { x.Key.Name }) : null));
                                return obj;
                            });
                        }
                    }

                    throw new Exception("Какие-то проблемы во время получения массива анонимных объектов.");
                }
                else
                {
                    Dapper.SqlMapper.SetTypeMap(typeof(TEntity), new DapperColumnAttributeTypeMapper<TEntity>());

                    var dynamicParameters = new DynamicParameters(parameters);
                    var results = Database.GetDbConnection().Query<TEntity>(queryText, parameters, buffered: cacheInLocal, commandTimeout: Database.GetCommandTimeout());
                    return results;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Возвращает результат выполнения запроса внутри контекста. В качестве запроса может выступать строка, объект и т.п. - зависит от возможностей конкретной реализации контекста.
        /// Возвращает количество строк, затронутых запросом..
        /// </summary>
        /// <param name="query">Запрос, который необходимо выполнить.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам запроса.
        /// Это может быть анонимный тип, например, для запроса с условием "DateChange=@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        public int ExecuteQuery(object query, object parameters = null)
        {
            try
            {
                if (query != null)
                {
                    var queryText = query.ToString();
                    var results = Database.GetDbConnection().Execute(queryText, parameters, commandTimeout: Database.GetCommandTimeout());

                    return results;
                }
                return 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region IDBOAccess
        /// <summary>
        /// Возвращает результат выполнения сохраненной процедуры. 
        /// Результат выполнения запроса возвращается в виде перечисления объектов типа <typeparamref name="TEntity"/>.
        /// Результат выполнения запроса не кешируется.
        /// </summary>
        /// <param name="procedure_name">Название сохраненной процедуры.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам сохраненной процедуры.
        /// Это может быть анонимный тип, например, для СП с параметром "@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        public IEnumerable<TEntity> StoredProcedure<TEntity>(string procedure_name, object parameters = null) 
            where TEntity : class
        {
            try
            {
                var results = Database.GetDbConnection().Query<TEntity>(procedure_name, parameters, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: Database.GetCommandTimeout());
                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Возвращает результат выполнения сохраненной процедуры, возвращающей несколько наборов данных. 
        /// Результат выполнения запроса возвращается в виде нескольких перечислений объектов указанных типов.
        /// Результат выполнения запроса не кешируется.
        /// </summary>
        /// <param name="procedure_name">Название сохраненной процедуры.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам сохраненной процедуры.
        /// Это может быть анонимный тип, например, для СП с параметром "@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        public Tuple<IEnumerable<TEntity1>, IEnumerable<TEntity2>> StoredProcedure<TEntity1, TEntity2>(string procedure_name, object parameters = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            try
            {
                using (var reader = Database.GetDbConnection().QueryMultiple(procedure_name, parameters, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: Database.GetCommandTimeout()))
                {
                    return new Tuple<IEnumerable<TEntity1>, IEnumerable<TEntity2>>(reader.Read<TEntity1>().ToList(), reader.Read<TEntity2>().ToList());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Возвращает результат выполнения сохраненной процедуры, возвращающей несколько наборов данных. 
        /// Результат выполнения запроса возвращается в виде нескольких перечислений объектов указанных типов.
        /// Результат выполнения запроса не кешируется.
        /// </summary>
        /// <param name="procedure_name">Название сохраненной процедуры.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам сохраненной процедуры.
        /// Это может быть анонимный тип, например, для СП с параметром "@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        public Tuple<IEnumerable<TEntity1>, IEnumerable<TEntity2>, IEnumerable<TEntity3>> StoredProcedure<TEntity1, TEntity2, TEntity3>(string procedure_name, object parameters = null)
            where TEntity1 : class
            where TEntity2 : class
            where TEntity3 : class
        {
            try
            {
                using (var reader = Database.GetDbConnection().QueryMultiple(procedure_name, parameters, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: Database.GetCommandTimeout()))
                {
                    return new Tuple<IEnumerable<TEntity1>, IEnumerable<TEntity2>, IEnumerable<TEntity3>>(reader.Read<TEntity1>().ToList(), reader.Read<TEntity2>().ToList(), reader.Read<TEntity3>().ToList());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Применение изменений
        /// <summary>
        /// Возвращает список всех измененных объектов.
        /// </summary>
        private Dictionary<EntityEntry, EntityState> GetChangedEntities()
        {
            var entities = ChangeTracker.Entries()
                            .Where(x => x.State != EntityState.Unchanged)
                            .ToDictionary(x => x, x => x.State);

            return entities;
        }

        /// <summary>
        /// Для каждого объекта, состояние которого было изменено в результате применения изменений, вызываются методы, помеченные 
        /// атрибутом <see cref="SavedInContextEventAttribute"/> для выполнения дополнительных действий после сохранения. 
        /// </summary>
        private void DetectSavedEntities(Dictionary<EntityEntry, EntityState> entities)
        {
            // Core.Items.MethodMarkCallerAttribute.CallMethodsInObjects<Core.Items.SavedInContextEventAttribute>(entities.Where(x => x.Value != x.Key.State));

            foreach (var pair in entities)
            {
                if (pair.Value != pair.Key.State)
                {
                    MethodMarkCallerAttribute.CallMethodsInObject<SavedInContextEventAttribute>(pair.Key.Entity);
                }
            }
        }

        /// <summary>
        /// Применяет все изменения базы данных, произведенные в контексте.
        /// После окончания операции для каждого затронутого объекта выполняются методы, помеченные атрибутом <see cref="SavedInContextEventAttribute"/>.
        /// Более подробную информацию см. <see cref="DbContext.SaveChanges()"/>. 
        /// </summary>
        /// <returns>Количество объектов, записанных в базу данных.</returns>
        public override int SaveChanges()
        {
            var entities = GetChangedEntities();
            try
            {
                foreach (var entity in entities)
                {
                    var validationContext = new ValidationContext(entity.Key.Entity);
                    Validator.ValidateObject(entity.Key.Entity, validationContext);
                }

                return base.SaveChanges();
            }
            finally
            {
                DetectSavedEntities(entities);
            }
        }

        /// <summary>
        /// Применяет все изменения базы данных, произведенные в контексте для указанного типа объектов <typeparamref name="TEntity"/>.
        /// После окончания операции для каждого затронутого объекта выполняются методы, помеченные атрибутом <see cref="SavedInContextEventAttribute"/>.
        /// Более подробную информацию см. <see cref="DbContext.SaveChanges()"/>. 
        /// </summary>
        /// <typeparam name="TEntity">Тип сущностей, для которых следует применить изменения.</typeparam>
        /// <returns>Количество объектов, записанных в базу данных.</returns>
        public int SaveChanges<TEntity>() where TEntity : class
        {
            return SaveChanges(typeof(TEntity));
        }

        /// <summary>
        /// Применяет все изменения базы данных, произведенные в контексте для указанного типа объектов <paramref name="entityType"/>.
        /// После окончания операции для каждого затронутого объекта выполняются методы, помеченные атрибутом <see cref="SavedInContextEventAttribute"/>.
        /// Более подробную информацию см. <see cref="DbContext.SaveChanges()"/>. 
        /// </summary>
        /// <param name="entityType">Тип сущностей, для которых следует применить изменения.</param>
        /// <returns>Количество объектов, записанных в базу данных.</returns>
        public int SaveChanges(Type entityType)
        {
            if (entityType == null) throw new ArgumentNullException(nameof(entityType), "Должен быть указан тип сущностей для сохранения.");

            var original = ChangeTracker.
                Entries().
                Where(x => !entityType.IsAssignableFrom(x.Entity.GetType()) && x.State != EntityState.Unchanged).
                Select(x => new { Entity = x, CurrentValues = x.CurrentValues.Clone() }).
                GroupBy(x => x.Entity.State).
                ToList();

            foreach (var entry in ChangeTracker.Entries().Where(x => !entityType.IsAssignableFrom(x.Entity.GetType())))
            {
                entry.State = EntityState.Unchanged;
            }

            var entities = GetChangedEntities();
            try
            {
                foreach (var entity in entities)
                {
                    var validationContext = new ValidationContext(entity.Key.Entity);
                    Validator.ValidateObject(entity.Key.Entity, validationContext);
                }

                return base.SaveChanges();
            }
            finally
            {
                foreach (var state in original)
                {
                    foreach (var entry in state)
                    {
                        entry.Entity.State = state.Key;
                        entry.Entity.CurrentValues.SetValues(entry.CurrentValues);
                    }
                }

                DetectSavedEntities(entities);
            }
        }

        /// <summary>
        /// Асинхронно применяет все изменения базы данных, произведенные в контексте.
        /// После окончания операции для каждого затронутого объекта выполняются методы, помеченные атрибутом <see cref="SavedInContextEventAttribute"/>.
        /// Более подробную информацию см. <see cref="DbContext.SaveChanges()"/>. 
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию сохранения. Результат задачи содержит количество объектов, записанных в базу данных.</returns>
        public Task<int> SaveChangesAsync()
        {
            var entities = GetChangedEntities();

            var task = base.SaveChangesAsync();
            return Task.Factory.StartNew<int>(() =>
            {
                try
                {
                    task.Wait();
                    var rows = task.Result;
                    return rows;
                }
                finally
                {
                    DetectSavedEntities(entities);
                }
            });
        }

        /// <summary>
        /// Асинхронно применяет все изменения базы данных, произведенные в контексте.
        /// После окончания операции для каждого сохраненного объекта выполняются методы, помеченные атрибутом <see cref="SavedInContextEventAttribute"/>.
        /// Более подробную информацию см. <see cref="DbContext.SaveChanges()"/>. 
        /// </summary>
        /// <param name="cancellationToken">Токен System.Threading.CancellationToken, который нужно отслеживать во время ожидания выполнения задачи.</param>
        /// <returns>Задача, представляющая асинхронную операцию сохранения. Результат задачи содержит количество объектов, записанных в базу данных.</returns>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            var entities = GetChangedEntities();

            var task = base.SaveChangesAsync(cancellationToken);
            return Task.Factory.StartNew<int>(() =>
            {
                try
                {
                    task.Wait();
                    var rows = task.Result;
                    return rows;
                }
                finally
                {
                    DetectSavedEntities(entities);
                }
            });
        }

        #endregion

        #region TransactionScope
        /// <summary>
        /// См. <see cref="TransactionScope.TransactionScope()"/>.
        /// </summary>
        public TransactionScope CreateScope()
        {
            return new TransactionScope();
        }

        /// <summary>
        /// См. <see cref="TransactionScope.TransactionScope(TransactionScopeOption)"/>.
        /// </summary>
        public TransactionScope CreateScope(TransactionScopeOption scopeOption)
        {
            return new TransactionScope(scopeOption);
        }

        /// <summary>
        /// См. <see cref="TransactionScope.TransactionScope(TransactionScopeOption, TimeSpan)"/>.
        /// </summary>
        public TransactionScope CreateScope(TransactionScopeOption scopeOption, TimeSpan scopeTimeout)
        {
            return new TransactionScope(scopeOption, scopeTimeout);
        }

        /// <summary>
        /// См. <see cref="TransactionScope.TransactionScope(TransactionScopeOption, TransactionOptions)"/>.
        /// </summary>
        public TransactionScope CreateScope(TransactionScopeOption scopeOption, TransactionOptions transactionOptions)
        {
            return new TransactionScope(scopeOption, transactionOptions);
        }
        #endregion

        #region Свойства
        /// <summary>
        /// Возвращает или задает таймаут выполнения запроса в миллисекундах.
        /// </summary>
        public int QueryTimeout
        {
            get => !Database.GetCommandTimeout().HasValue ? 30000 : Database.GetCommandTimeout().Value * 1000;
            set => Database.SetCommandTimeout(TimeSpan.FromMilliseconds(value));
        }
        #endregion

    }
}
