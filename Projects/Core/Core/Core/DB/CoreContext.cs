using Microsoft.EntityFrameworkCore;
using OnUtils.Data;
using OnUtils.Data.UnitOfWork;

namespace OnXap.Core.Db
{
    using Data;

    /// <summary>
    /// ������� �������� ���-����������, ��������� ������������ ������ �����������.
    /// </summary>
    public class CoreContextBase : DbContextBase
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = DataAccessManager.GetConnectionStringResolver().ResolveConnectionStringForDataContext(null);
            optionsBuilder.UseSqlServer(connectionString);
        }

        /// <summary>
        /// ��. <see cref="UnitOfWorkBase.OnModelCreating(IModelAccessor)"/>.
        /// </summary>
        protected virtual void OnModelCreatingCustom(IModelAccessor modelAccessor)
        {
        }

        internal static string ConnectionString { get; set; }
    }

    /// <summary>
    /// �������� ��������, ���������� ��� �������� ����. 
    /// </summary>
    /// <seealso cref="CoreContextBase"/>
    public class CoreContext : CoreContextBase
    {
#pragma warning disable CS1591 // todo ������ �����������.
        public DbSet<ModuleConfig> Module { get; set; }

        public DbSet<ItemType> ItemType { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Role> Role { get; set; }
        public DbSet<RoleUser> RoleUser { get; set; }
        public DbSet<RolePermission> RolePermission { get; set; }
    }
}
