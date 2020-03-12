using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace OnXap.Core.Db
{
    using Data;

    /// <summary>
    /// Ѕазовый контекст приложени€, корректно определ€ющий строку подключени€.
    /// </summary>
    public class CoreContextBase : DbContextBase
    {
        #region OnConfiguring
        internal override void OnConfiguringInternal(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.
                UseSqlServer(ConnectionStringFactory()).
                ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning, RelationalEventId.QueryPossibleExceptionWithAggregateOperatorWarning, RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning));
            OnContextConfiguring(optionsBuilder);
        }

        /// <summary>
        /// Override this method to configure the database (and other options) to be used for this context. 
        /// This method is called for each instance of the context that is created. The base implementation does nothing. 
        /// In situations where an instance of <see cref="DbContextOptions"/> may or may not have been passed to the constructor, you can use <see cref="DbContextOptionsBuilder.IsConfigured"/> to 
        /// determine if the options have already been set, and skip some or all of the logic in <see cref="OnContextConfiguring(DbContextOptionsBuilder)"/>.
        /// </summary>
        /// <param name="optionsBuilder">
        /// A builder used to create or modify options for this context. Databases (and other extensions) typically define extension methods on this object that 
        /// allow you to configure the context.
        /// </param>
        protected virtual void OnContextConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
        #endregion

        #region OnModelCreating
        internal override void OnModelCreatingInternal(ModelBuilder modelBuilder)
        {
            OnContextModelCreating(modelBuilder);
        }

        /// <summary>
        /// Override this method to further configure the model that was discovered by convention from the entity types exposed in <see cref="DbSet{TEntity}"/> 
        /// properties on your derived context. The resulting model may be cached and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">
        /// The builder being used to construct the model for this context. Databases (and other extensions) typically define extension 
        /// methods on this object that allow you to configure aspects of the model that are specific to a given database.
        /// </param>
        /// <remarks>
        /// If a model is explicitly set on the options for this context (via <see cref="DbContextOptionsBuilder.UseModel(IModel)"/>) then this method will not be run.
        /// </remarks>
        protected virtual void OnContextModelCreating(ModelBuilder modelBuilder)
        {
        }
        #endregion

        #region —войства
        internal static Func<string> ConnectionStringFactory { get; set; }
        #endregion
    }

    /// <summary>
    /// ќсновной контекст, содержащий все сущности €дра. 
    /// </summary>
    /// <seealso cref="CoreContextBase"/>
    public class CoreContext : CoreContextBase
    {
#pragma warning disable CS1591 // todo внести комментарии.
        public DbSet<ModuleConfig> Module { get; set; }

        public DbSet<ItemType> ItemType { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Role> Role { get; set; }
        public DbSet<RoleUser> RoleUser { get; set; }
        public DbSet<RolePermission> RolePermission { get; set; }
    }
}
