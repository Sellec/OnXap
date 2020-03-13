using System;
using System.Collections.Generic;
using System.Text;
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
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using OnUtils.Architecture.AppCore;

namespace OnXap.Core.MetadataObject.Db
{
    using Core.Db;

    /// <summary>
    /// Базовый контекст приложения для работы с объектами метаданных.
    /// Для корректной работы с метаданными все контексты, наследующие данный тип, должны создаваться через ядро.
    /// </summary>
    /// <remarks>
    /// Для контекстов-наследников необязательна регистрация через привязку типов, если контекст-наследник является неабстрактным и имеет беспараметрический конструктор.
    /// Такие типы обрабатываются в <see cref="MetadataObjectDbContextResolver"/>.
    /// </remarks>
    public abstract class MetadataObjectDbContextBase : CoreContextBase, IComponent, IComponentStartable, IComponentTransient
    {
        class InternalCoreComponent : CoreComponentBase
        {
        }

        private static ModuleBuilder _moduleBuilder = null;
        private InternalCoreComponent _internalCoreComponent = new InternalCoreComponent();

        static MetadataObjectDbContextBase()
        {
            var aName = new AssemblyName("DynamicAssemblyMetadataObject");
            var ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
            var mb = ab.DefineDynamicModule(aName.Name);
            _moduleBuilder = mb;
        }

        private void CheckAsComponent()
        {
            if (_internalCoreComponent.GetState() != CoreComponentState.Started) throw new InvalidOperationException("Контекст не был корректно создан или определен в качестве компонента ядра.");
        }

        #region OnConfiguring
        internal override void OnConfiguringInternal(DbContextOptionsBuilder optionsBuilder)
        {
            CheckAsComponent();

            _internalCoreComponent.AppCore.DbConfigurationBuilder.OnConfigureEntityFrameworkCore(optionsBuilder);
            OnContextConfiguring(optionsBuilder);
        }
        #endregion

        #region OnModelCreating
        internal override void OnModelCreatingInternal(ModelBuilder modelBuilder)
        {
            CheckAsComponent();

            foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(x => x.ClrType != null && typeof(MetadataObject).IsAssignableFrom(x.ClrType)).ToList())
            {
                var tb = _moduleBuilder.DefineType(DateTime.Now.Ticks.ToString(), TypeAttributes.NotPublic, typeof(MetadataObjectPropertyData));
                var t = tb.CreateTypeInfo();
                var type = (Type)t;
                typeof(MetadataObjectDbContextBase).
                    GetMethod(nameof(OnModelCreatingApplyCustomType), BindingFlags.NonPublic | BindingFlags.Instance).
                    MakeGenericMethod(t, entityType.ClrType).
                    Invoke(this, new object[] { modelBuilder });
            }

            base.OnModelCreatingInternal(modelBuilder);
        }

        private void OnModelCreatingApplyCustomType<TMetadataObjectPropertyData, TEntity>(ModelBuilder modelBuilder)
            where TMetadataObjectPropertyData : MetadataObjectPropertyData
            where TEntity : MetadataObject
        {
            var idItemType = Items.ItemTypeFactory.GetItemType<TEntity>().IdItemType;

            modelBuilder.Entity<TEntity>().
                HasMany(x => x.SourcePropertyData).
                WithOne().
                HasPrincipalKey(x => x.ID).
                HasForeignKey(x => x.IdItem);

            modelBuilder.Entity<MetadataObjectPropertyData>().
                HasKey(x => new { x.IdField, x.IdFieldValue, x.IdItem, x.IdItemType, x.IdSchemeItem });

            modelBuilder.Entity<MetadataObjectPropertyData>().
                HasDiscriminator(x => x.IdItemType).
                HasValue<MetadataObjectPropertyData>(0).
                HasValue<TMetadataObjectPropertyData>(idItemType);
        }
        #endregion

        #region IComponentStartable
        void IComponentStartable<OnXApplication>.Start(OnXApplication appCore)
        {
            ((IComponentStartable)_internalCoreComponent).Start(appCore);
        }
        #endregion

        #region IComponent
        void IComponent<OnXApplication>.Stop()
        {
            ((IComponent)_internalCoreComponent).Stop();
        }

        CoreComponentState IComponent<OnXApplication>.GetState()
        {
            return _internalCoreComponent.GetState();
        }

        OnXApplication IComponent<OnXApplication>.GetAppCore()
        {
            return _internalCoreComponent.GetAppCore();
        }
        #endregion


    }
}
