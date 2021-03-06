﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace OnXap.Modules.ItemsCustomize.Scheme
{
    using Core.Items;

    /// <summary>
    /// Описывает поле или свойство, являющееся ключом для получения контейнера схемы <see cref="SchemeItem"/> для объекта. 
    /// Например, если пометить этим атрибутом свойство Category для Goods, то при поиске схемы полей для объекта Goods 
    /// будет использовано значение поля Category в качестве <see cref="SchemeItem.IdItem"/> и значение <see cref="IdItemType"/> в качестве <see cref="SchemeItem.IdItemType"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SchemeItemAttribute : Attribute
    {
        private static ConcurrentDictionary<Type, Tuple<MemberInfo, int>> _knownTypes = new ConcurrentDictionary<Type, Tuple<MemberInfo, int>>();
        private Lazy<int> _idItemType = null;

        /// <summary>
        /// Создает новый экземпляр атрибута.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Возникает, если передано некорректное значение идентификатора типа объекта.</exception>
        public SchemeItemAttribute(int idItemType)
        {
            if (idItemType <= 0) throw new ArgumentOutOfRangeException(nameof(idItemType), "Идентификатор типа объекта должен быть больше нуля.");
            _idItemType = new Lazy<int>(() => idItemType);
        }

        /// <summary>
        /// Создает новый экземпляр атрибута.
        /// </summary>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="itemType"/> равен null.</exception>
        public SchemeItemAttribute(Type itemType)
        {
            if (itemType == null) throw new ArgumentNullException(nameof(itemType));
            _idItemType = new Lazy<int>(() => ItemTypeFactory.ItemTypes.Where(x => x.Key == "TYPEKEY_" + itemType.FullName).Select(x => x.Value).FirstOrDefault()?.IdItemType ?? 0);
        }

        /// <summary>
        /// Тип контейнера схемы.
        /// </summary>
        public int IdItemType
        {
            get => _idItemType.Value;
        }

        /// <summary>
        /// Возвращает контейнер схемы для указанного объекта.
        /// </summary>
        public static SchemeItem GetValueFromObject(object obj)
        {
            var propfield = _knownTypes.GetOrAdd(obj.GetType(), KnownTypePrepare);
            if (propfield != null)
            {
                if (propfield.Item1.MemberType == MemberTypes.Field)
                {
                    return new SchemeItem((int)(propfield.Item1 as FieldInfo).GetValue(obj), propfield.Item2);
                }
                if (propfield.Item1.MemberType == MemberTypes.Property)
                {
                    return new SchemeItem((int)(propfield.Item1 as PropertyInfo).GetValue(obj), propfield.Item2);
                }
            }

            return new SchemeItem(0, ItemTypeAttribute.GetValueFromObject(obj));
            //return SchemeItem.Default;
        }

        private static Tuple<MemberInfo, int> KnownTypePrepare(Type type)
        {
            var ms = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty)
                        .Select(x => new { Member = x, Attribute = x.GetCustomAttribute<SchemeItemAttribute>(true) })
                        .Where(x => x.Attribute != null);

            if (ms.Count() == 0) return null;
            else if (ms.Count() > 1)
            {
                throw new AmbiguousMatchException(string.Format("Для типа '{0}' объявлено несколько свойств или полей с атрибутом '{1}'. Только один член класса может быть помечен этим атрибутом.", type.FullName, typeof(SchemeItem).FullName));
            }
            else
            {
                var member = ms.First();
                var typeError = false;
                if (member.Member.MemberType == MemberTypes.Property && (member.Member as PropertyInfo).PropertyType != typeof(int)) typeError = true;
                if (member.Member.MemberType == MemberTypes.Field && (member.Member as FieldInfo).FieldType != typeof(int)) typeError = true;
                if (typeError) throw new AmbiguousMatchException(string.Format("Поле или свойство, помеченное атрибутом '{0}', должно иметь возвращаемый тип int.", typeof(SchemeItem).FullName));

                return new Tuple<MemberInfo, int>(member.Member, member.Attribute.IdItemType);
            }
        }
    }
}
