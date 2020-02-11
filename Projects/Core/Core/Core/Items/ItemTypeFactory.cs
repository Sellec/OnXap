using OnUtils.Data;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace OnXap.Core.Items
{
    using Core.Db;
    using Utils;

    /// <summary>
    /// Предоставляет методы для работы с типами объектов.
    /// </summary>
    public class ItemTypeFactory //: ProvidersFactoryStartup<IItemTypeProvider, ItemTypeFactory>
    {
        /// <summary>
        /// Значение, обозначающее, что идентификатор типа объектов не найден.
        /// </summary>
        public const ItemType NotFound = null;
        private static Lazy<Tuple<DateTime, ConcurrentDictionary<string, ItemType>>> _itemsTypes = null;
        private static ConcurrentDictionary<int, Type> _itemsClsTypes = new ConcurrentDictionary<int, Type>();

        private static Tuple<DateTime, ConcurrentDictionary<string, ItemType>> ItemsTypesProvide()
        {
            try
            {
                var types = new ConcurrentDictionary<string, ItemType>();

                using (var db = new UnitOfWork<ItemType>())
                {
                    db.Repo1.Where(x => !string.IsNullOrEmpty(x.UniqueKey)).ForEach(x => types[x.UniqueKey] = x);
                }

                var expires = DateTime.Now.AddMinutes(2);

                //Debug.WriteLineNoLog("ItemTypeFactory: generate new cache with {0} types, expires at {1}", types.Count, expires.ToString("yyyy-MM-dd HH:mm:ss"));

                return new Tuple<DateTime, ConcurrentDictionary<string, ItemType>>(DateTime.Now.AddMinutes(2), types);
            }
            catch
            {
                return new Tuple<DateTime, ConcurrentDictionary<string, ItemType>>(DateTime.Now, new ConcurrentDictionary<string, ItemType>());
            }
        }

        /// <summary>
        /// Возвращает тип объектов для идентификатора <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Идентификатор, для которого следует получить тип объектов.</param>
        public static ItemType GetItemType(int type)
        {
            if (type <= 0) return NotFound;

            var _r = ItemTypes.Where(x => x.Value.IdItemType == type).Select(x => x.Value).FirstOrDefault();
            if (_r == null)
                using (var db = new UnitOfWork<ItemType>())
                    _r = db.Repo1.Where(x => x.IdItemType == type).FirstOrDefault();

            if (_r != null) return _r;

            return NotFound;
        }

        /// <summary>
        /// Возвращает идентификатор указанного типа объектов <typeparamref name="TItemType"/>.
        /// </summary>
        /// <typeparam name="TItemType">Тип объектов, для которого следует получить идентификатор.</typeparam>
        public static ItemType GetItemType<TItemType>()
        {
            return GetItemType(typeof(TItemType));
        }

        /// <summary>
        /// Возвращает идентификатор указанного типа объектов <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Тип объектов, для которого следует получить идентификатор.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="type"/> равен null.</exception>
        public static ItemType GetItemType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type), "Следует указать тип объекта");

            var t = type;
            var aliasTypeAttribute = type.GetCustomAttribute<ItemTypeAliasAttribute>(true);
            if (aliasTypeAttribute != null) return GetItemType(aliasTypeAttribute.AliasType);

            var itemTypeAttribute = type.GetCustomAttribute<ItemTypeAttribute>(true);
            if (itemTypeAttribute != null) return GetItemType(itemTypeAttribute.IdItemType);

            if (t.FullName.StartsWith("System.Data.Entity.DynamicProxies.")) t = t.BaseType;

            var caption = t.Name;
            var displayAttribute = t.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null && !string.IsNullOrEmpty(displayAttribute.Name)) caption = displayAttribute.Name;
            var displayNameAttribute = t.GetCustomAttribute<DisplayNameAttribute>();
            if (displayNameAttribute != null && !string.IsNullOrEmpty(displayNameAttribute.DisplayName)) caption = displayNameAttribute.DisplayName;

            var fullName = TypeNameHelper.GetFullNameCleared(t);
            var ret = GetOrAdd(caption, "TYPEKEY_" + fullName, true);
            _itemsClsTypes[ret.IdItemType] = t;
            return ret;
        }

        /// <summary>
        /// Возвращает cls-тип для указанного типа объектов.
        /// </summary>
        /// <param name="idItemType"></param>
        /// <returns></returns>
        public static Type GetClsType(int idItemType)
        {
            var itemType = GetItemType(idItemType);
            if (itemType == null) throw new InvalidOperationException("Неизвестный тип объектов.");

            var funcGet = new Func<Type>(() =>
            {
                Type type = null;

                if (itemType.UniqueKey.StartsWith("TYPEKEY_"))
                {
                    var typeName = itemType.UniqueKey.Substring("TYPEKEY_".Length);
                    if (!string.IsNullOrEmpty(typeName))
                    {
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            foreach (var type2 in assembly.GetTypes())
                            {
                                var typeName2 = TypeNameHelper.GetFullNameCleared(type2);
                                if (typeName2 == typeName)
                                {
                                    type = type2;
                                    break;
                                }
                            }
                            if (type != null) break;
                        }
                    }
                }

                return type;
            });

            var value = _itemsClsTypes.GetOrAdd(
                itemType.IdItemType,
                k => funcGet()
            );
            return value;
        }

        private static ItemType GetOrAdd(string caption, string uniqueKey, bool registerIfNoFound)
        {
            var r = ItemTypes.Where(x => x.Value.UniqueKey == uniqueKey).Select(x => x.Value).FirstOrDefault();
            if (r == null)
            {
                using (var db = new UnitOfWork<ItemType>())
                {
                    r = db.Repo1.Where(x => x.UniqueKey == uniqueKey).FirstOrDefault();
                    if (r == null && registerIfNoFound)
                    {
                        var r_ = new ItemType() { NameItemType = caption, UniqueKey = uniqueKey };
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
                using (var db = new UnitOfWork<ItemType>())
                {
                    db.Repo1.InsertOrDuplicateUpdate(r.ToEnumerable(), new UpsertField(nameof(r.NameItemType)));
                }
            }

            return r;
        }

        internal static ConcurrentDictionary<string, ItemType> ItemTypes
        {
            get
            {
                if (_itemsTypes == null || (_itemsTypes.IsValueCreated && _itemsTypes.Value.Item1 <= DateTime.Now)) _itemsTypes = new Lazy<Tuple<DateTime, ConcurrentDictionary<string, ItemType>>>(ItemsTypesProvide);
                return _itemsTypes.Value.Item2;
            }
        }
    }
}
