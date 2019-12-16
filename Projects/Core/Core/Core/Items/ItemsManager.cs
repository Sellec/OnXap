using OnUtils.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnXap.Core.Items
{
    using Journaling;
    using Modules;

    /// <summary>
    /// Менеджер, управляющий сущностями и типами сущностей.
    /// </summary>
    public class ItemsManager : 
        CoreComponentBase, 
        IComponentSingleton, 
        IUnitOfWorkAccessor<DataContext>,
        ITypedJournalComponent<ItemsManager>
    {
        private class ParentsInternal
        {
            public int item;
            public int type;
            public int parent;
            public int level;
        }

        private ConcurrentDictionary<Type, Tuple<DB.ItemType, Type>> _itemTypeModuleType;

        private ConcurrentDictionary<ItemKey, object> _currentProcessingItemKey = new ConcurrentDictionary<ItemKey, object>();
        private Guid _latestUsedLinkId = Guid.Empty;
        private object _latestUsedLinkSyncRoot = new object();

        /// <summary>
        /// </summary>
        public ItemsManager()
        {
            _itemTypeModuleType = new ConcurrentDictionary<Type, Tuple<DB.ItemType, Type>>();
        }

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion

        /// <summary>
        /// Позволяет сохранить список взаимосвязей родитель:потомок для типа сущностей <paramref name="idItemType"/> (см. <see cref="ItemTypeFactory"/>).
        /// </summary>
        /// <param name="module">Модуль</param>
        /// <param name="relationsList">Список взаимосвязей</param>
        /// <param name="idItemType">Идентификатор типа сущности (см. <see cref="DB.ItemType.IdItemType"/>).</param>
        /// <returns>Возвращает true, если сохранение прошло успешно и false, если возникла ошибка. Возвращает true, если <paramref name="relationsList"/> пуст.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="module"/> равен null.</exception>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="relationsList"/> равен null.</exception>
        [ApiReversible]
        public bool SaveChildToParentRelations(ModuleCore module, int idItemType, IEnumerable<ChildToParentRelation> relationsList)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));
            if (relationsList == null) throw new ArgumentNullException(nameof(relationsList));

            try
            {
                relationsList = relationsList.ToList();
                if (relationsList.Count() == 0) return true;

                var toBase = new List<ParentsInternal>();

                toBase.Add(new ParentsInternal()
                {
                    item = 0,
                    type = idItemType,
                    parent = 0,
                    level = 0,
                });

                var levelMax = 100;
                foreach (var pair in relationsList)
                {
                    var level = 0;

                    toBase.Add(new ParentsInternal()
                    {
                        item = pair.IdChild,
                        type = idItemType,
                        parent = pair.IdChild,
                        level = level++,
                    });

                    var s = pair.IdParent > 0 ? pair.IdParent : 0;

                    while (s > 0 || level <= levelMax)
                    {
                        toBase.Add(new ParentsInternal()
                        {
                            item = pair.IdChild,
                            type = idItemType,
                            parent = s,
                            level = level++,
                        });

                        if (s == 0 || level == levelMax) break;

                        //$s = 0;
                        //if (isset($items[$s]) && $items[$s] > 0) $s = $items[$s];
                        var s_ = relationsList.Where(x => x.IdChild == s).FirstOrDefault();
                        if (s_ != null) s = s_.IdParent;
                        else if (s_ == null && s > 0) break;
                        s = s > 0 ? s : 0;
                    }
                }

                using (var db = this.CreateUnitOfWork())
                using (var scope = db.CreateScope())
                {
                    foreach (var item in toBase.GroupBy(x => $"{x.item}_{x.type}_{x.parent}", x => x).Select(x => x.First()))
                    {
                        db.ItemParent.Add(new DB.ItemParent()
                        {
                            IdModule = module.IdModule,
                            IdItem = item.item,
                            IdItemType = item.type,
                            IdParentItem = item.parent,
                            IdLevel = item.level
                        });
                    }

                    db.DataContext.ExecuteQuery($"DELETE FROM ItemParent WHERE IdModule='{module.IdModule}' AND IdItemType='{idItemType}'");//" AND IdItem IN (".implode(', ', $ids).")");
                    if (toBase.Count > 0)
                    {
                        db.SaveChanges();
                        scope.Commit();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                this.RegisterEvent(EventType.Error, "Не удалось сохранить список пар", $"Модуль: {module.GetType().FullName}\r\nСписок пар: {relationsList.Count()}\r\nТип сущностей: {idItemType}.", null, ex);
                return false;
            }

        }

        /// <summary>
        /// Связывает тип объекта <typeparamref name="TItemBase"/> с модулем типа <typeparamref name="TModule"/>.
        /// </summary>
        /// <seealso cref="ModuleCore{TSelfReference}.QueryType"/>
        /// <seealso cref="ItemTypeAliasAttribute"/>
        public void RegisterModuleItemType<TItemBase, TModule>()
            where TItemBase : ItemBase
            where TModule : ModuleCore
        {
            var module = AppCore.Get<TModule>();
            RegisterModuleItemType<TItemBase, TModule>(module);
        }

        /// <summary>
        /// Связывает тип объекта <typeparamref name="TItemBase"/> с модулем типа <typeparamref name="TModule"/>.
        /// </summary>
        /// <seealso cref="ModuleCore{TSelfReference}.QueryType"/>
        /// <seealso cref="ItemTypeAliasAttribute"/>
        internal void RegisterModuleItemType<TItemBase, TModule>(TModule module)
            where TItemBase : ItemBase
            where TModule : ModuleCore
        {
            var type = typeof(TItemBase);
            var typesList = new List<Type>() { type };

            while (true)
            {
                var aliasTypeAttribute = type.GetCustomAttribute<ItemTypeAliasAttribute>(true);
                if (aliasTypeAttribute != null)
                {
                    if (typesList.Contains(aliasTypeAttribute.AliasType)) throw new InvalidOperationException($"Нижестоящий тип '{type.FullName}' ссылается на вышестоящий '{aliasTypeAttribute.AliasType}'.");
                    if (type != aliasTypeAttribute.AliasType)
                    {
                        type = aliasTypeAttribute.AliasType;
                        typesList.Add(type);
                        continue;
                    }
                }

                break;
            }

            var itemType = ItemTypeFactory.GetItemType(type);
            _itemTypeModuleType[type] = new Tuple<DB.ItemType, Type>(itemType, typeof(TModule));

            using (var db = new DB.CoreContext())
            {
                var query = db.ItemParent.Where(x => x.IdItemType == itemType.IdItemType && x.IdModule == module.IdModule);
                if (query.Count() == 0) SaveChildToParentRelations(module, itemType.IdItemType, new ChildToParentRelation() { IdChild = 0, IdParent = 0 }.ToEnumerable());
            }
        }

        /// <summary>
        /// Возвращает список типов объектов, зарегистрированных для модуля <typeparamref name="TModule"/>.
        /// </summary>
        /// <seealso cref="ModuleCore{TSelfReference}.QueryType"/>
        public List<DB.ItemType> GetModuleItemTypes<TModule>()
        {
            return _itemTypeModuleType.Where(x => x.Value.Item2 == typeof(TModule)).Select(x => x.Value.Item1).ToList();
        }

        /// <summary>
        /// Возвращает модуль для типа объекта <typeparamref name="TItemBase"/>.
        /// </summary>
        /// <param name="instance">Необязательный параметр, предназначен для удобства вызова метода с передачей ссылки на экземпляр нужного типа.</param>
        /// <seealso cref="ModuleCore{TSelfReference}.QueryType"/>
        /// <seealso cref="ItemTypeAliasAttribute"/>
        public ModuleCore GetModuleForItemType<TItemBase>(TItemBase instance = null)
            where TItemBase : ItemBase
        {
            return GetModuleForItemType(typeof(TItemBase));
        }

        /// <summary>
        /// Возвращает модуль для типа объекта <paramref name="itemType"/>.
        /// </summary>
        /// <seealso cref="ModuleCore{TSelfReference}.QueryType"/>
        /// <seealso cref="ItemTypeAliasAttribute"/>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="itemType"/> равен null.</exception>
        /// <exception cref="ArgumentException">Возникает, если <paramref name="itemType"/> не наследуется от <see cref="ItemBase"/>.</exception>
        public ModuleCore GetModuleForItemType(Type itemType)
        {
            if (itemType == null) throw new ArgumentNullException();
            if (!typeof(ItemBase).IsAssignableFrom(itemType)) throw new ArgumentException();

            var type = itemType;
            var typesList = new List<Type>() { type };

            while (true)
            {
                var aliasTypeAttribute = type.GetCustomAttribute<ItemTypeAliasAttribute>(true);
                if (aliasTypeAttribute != null)
                {
                    if (typesList.Contains(aliasTypeAttribute.AliasType)) throw new InvalidOperationException($"Нижестоящий тип '{type.FullName}' ссылается на вышестоящий '{aliasTypeAttribute.AliasType}'.");
                    if (type != aliasTypeAttribute.AliasType)
                    {
                        type = aliasTypeAttribute.AliasType;
                        continue;
                    }
                }

                break;
            }

            return _itemTypeModuleType.TryGetValue(type, out var moduleType) ? AppCore.Get<ModuleCore>(moduleType.Item2) : null;
        }

        /// <summary>
        /// Регистрирует ссылку для объекта с указанными параметрами <paramref name="itemKey"/> и возвращает идентификатор ссылки.
        /// Если ссылка для объекта уже зарегистрирована, то сразу возвращает идентификатор ссылки.
        /// </summary>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="itemKey"/> равен null.</exception>
        /// <exception cref="InvalidOperationException">Возникает в случае ошибки регистрации ссылки. Подробности регистрируются в журнале менеджера.</exception>
        public Guid RegisterItemLink(ItemKey itemKey)
        {
            if (itemKey == null) throw new ArgumentNullException(nameof(itemKey));

            try
            {
                var currentUserId = AppCore.GetUserContextManager().GetCurrentUserContext()?.IdUser;
                if (currentUserId <= 0) currentUserId = null;

                Guid guid;
                lock (_latestUsedLinkSyncRoot)
                {
                    int i = 1;
                    for (; i <= 5; i++)
                    {
                        guid = DateTime.Now.Ticks.ToString().GenerateGuid();
                        if (guid != _latestUsedLinkId)
                        {
                            _latestUsedLinkId = guid;
                            break;
                        }
                    }
                    if (i == 5) throw new InvalidProgramException("Не удается сгенерировать уникальный GUID.");
                }

                using (var db = new DataContext())
                {
                    int i = 1;
                    for (; i <= 5; i++)
                    {
                        try
                        {
                            var query = db.ItemLink.Where(x => x.ItemIdType == itemKey.IdType && x.ItemId == itemKey.IdItem && x.ItemKey == itemKey.Key);
                            var data = query.FirstOrDefault();
                            if (data != null) return data.LinkId;

                            db.ItemLink.AddOrUpdate(
                                x => new
                                {
                                    x.ItemIdType,
                                    x.ItemId,
                                    x.ItemKey
                                },
                                new DB.ItemLink()
                                {
                                    ItemIdType = itemKey.IdType,
                                    ItemId = itemKey.IdItem,
                                    ItemKey = itemKey.Key,
                                    LinkId = guid,
                                    IdUser = currentUserId,
                                    DateCreate = DateTime.Now
                                }
                            );
                            db.SaveChanges();
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (i == 5)
                            {
                                this.RegisterEvent(EventType.Error, "Ошибка регистрации ссылки", $"Данные объекта: {itemKey}", ex);
                                throw new InvalidOperationException("Возникла ошибка во время регистрации ссылки.");
                                break;
                            }
                        }
                    }
                }
                return guid;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.RegisterEvent(EventType.Error, "Ошибка регистрации ссылки", $"Данные объекта: {itemKey}", ex);
                throw new InvalidOperationException("Возникла ошибка во время регистрации ссылки.");
            }
        }
    }
}
