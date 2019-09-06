﻿using OnUtils.Data;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace OnXap.Core.Items
{
    using Utils;

    /// <summary>
    /// Предоставляет методы для работы с типами объектов.
    /// </summary>
    public class ItemTypeFactory //: ProvidersFactoryStartup<IItemTypeProvider, ItemTypeFactory>
    {
        /// <summary>
        /// Значение, обозначающее, что идентификатор типа объектов не найден.
        /// </summary>
        public const DB.ItemType NotFound = null;

        private static Lazy<Tuple<DateTime, ConcurrentDictionary<string, DB.ItemType>>> _itemsTypes = null;

        private static Tuple<DateTime, ConcurrentDictionary<string, DB.ItemType>> ItemsTypesProvide()
        {
            try
            {
                var types = new ConcurrentDictionary<string, DB.ItemType>();

                using (var db = new UnitOfWork<DB.ItemType>())
                {
                    db.Repo1.Where(x => !string.IsNullOrEmpty(x.UniqueKey)).ForEach(x => types[x.UniqueKey] = x);
                }

                var expires = DateTime.Now.AddMinutes(2);

                //Debug.WriteLineNoLog("ItemTypeFactory: generate new cache with {0} types, expires at {1}", types.Count, expires.ToString("yyyy-MM-dd HH:mm:ss"));

                return new Tuple<DateTime, ConcurrentDictionary<string, DB.ItemType>>(DateTime.Now.AddMinutes(2), types);
            }
            catch
            {
                return new Tuple<DateTime, ConcurrentDictionary<string, DB.ItemType>>(DateTime.Now, new ConcurrentDictionary<string, DB.ItemType>());
            }
        }

        /// <summary>
        /// Возвращает тип объектов для идентификатора <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Идентификатор, для которого следует получить тип объектов.</param>
        public static DB.ItemType GetItemType(int type)
        {
            if (type <= 0) return NotFound;

            var _r = ItemTypes.Where(x => x.Value.IdItemType == type).Select(x => x.Value).FirstOrDefault();
            if (_r == null)
                using (var db = new UnitOfWork<DB.ItemType>())
                    _r = db.Repo1.Where(x => x.IdItemType == type).FirstOrDefault();

            if (_r != null) return _r;

            return NotFound;
        }

        /// <summary>
        /// Возвращает идентификатор указанного типа объектов <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Тип объектов, для которого следует получить идентификатор.</param>
        public static DB.ItemType GetItemType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type), "Следует указать тип объекта");

            var t = type;
            var aliasTypeAttribute = type.GetCustomAttribute<ItemTypeAliasAttribute>(true);
            if (aliasTypeAttribute != null) return GetItemType(aliasTypeAttribute.AliasType);

            var itemTypeAttribute = type.GetCustomAttribute<ItemTypeAttribute>(true);
            if (itemTypeAttribute != null) return GetItemType(itemTypeAttribute.IdItemType);

            if (t.FullName.StartsWith("System.Data.Entity.DynamicProxies.")) t = t.BaseType;

            var caption = t.Name;
            var displayName = t.GetCustomAttribute<DisplayNameAttribute>();
            if (displayName != null && !string.IsNullOrEmpty(displayName.DisplayName)) caption = displayName.DisplayName;

            var fullName = TypeNameHelper.GetFullNameCleared(t);
            return GetOrAdd(caption, "TYPEKEY_" + fullName, true);
        }

        /// <summary>
        /// Возвращает идентификатор для указанных ключа и названия типа.
        /// </summary>
        public static DB.ItemType GetItemType(string typeKey, string caption)
        {
            if (string.IsNullOrEmpty(caption)) throw new ArgumentNullException(nameof(caption), "Название типа не должно быть пустым.");
            if (string.IsNullOrEmpty(typeKey)) throw new ArgumentNullException(nameof(typeKey), "Ключ типа должен быть пустым.");
            return GetOrAdd(caption, "CUSTOMKEY_" + typeKey, true);
        }

        private static DB.ItemType GetOrAdd(string caption, string uniqueKey, bool registerIfNoFound)
        {
            var r = ItemTypes.Where(x => x.Value.UniqueKey == uniqueKey).Select(x => x.Value).FirstOrDefault();
            if (r == null)
            {
                using (var db = new UnitOfWork<DB.ItemType>())
                {
                    r = db.Repo1.Where(x => x.UniqueKey == uniqueKey).FirstOrDefault();
                    if (r == null && registerIfNoFound)
                    {
                        var r_ = new DB.ItemType() { NameItemType = caption, UniqueKey = uniqueKey };
                        db.Repo1.AddOrUpdate(x => x.UniqueKey, r_);
                        db.SaveChanges();

                        r = r_;
                    }
                    ItemTypes[uniqueKey] = r;
                }
            }
            else if (r.NameItemType != caption)
            {
                r.NameItemType = caption;
                using (var db = new UnitOfWork<DB.ItemType>())
                {
                    db.Repo1.InsertOrDuplicateUpdate(r.ToEnumerable(), new UpsertField(nameof(r.NameItemType)));
                }
            }

            return r;
        }

        internal static ConcurrentDictionary<string, DB.ItemType> ItemTypes
        {
            get
            {
                if (_itemsTypes == null || (_itemsTypes.IsValueCreated && _itemsTypes.Value.Item1 <= DateTime.Now)) _itemsTypes = new Lazy<Tuple<DateTime, ConcurrentDictionary<string, DB.ItemType>>>(ItemsTypesProvide);
                return _itemsTypes.Value.Item2;
            }
        }
    }
}
