using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace OnXap.Core.MetadataObject
{
    /// <summary>
    /// Объект метаданных с дополнительными настраиваемыми свойствами.
    /// </summary>
    public abstract class MetadataObject
    {
        private Lazy<MetadataObjectPropertyCollection> _propertyCollection = null;

        /// <summary>
        /// Создает новый экземпляр объекта.
        /// </summary>
        public MetadataObject()
        {
            _propertyCollection = new Lazy<MetadataObjectPropertyCollection>(() => GeneratePropertyCollection());
        }

        private MetadataObjectPropertyCollection GeneratePropertyCollection()
        {
            if (DbContext == null) throw new InvalidOperationException("Объект метаданных должен быть присоединен или порожден контекстом, поддерживающим объекты метаданных.");
            return new MetadataObjectPropertyCollection(SourcePropertyData);
        }

        #region Свойства
        /// <summary>
        /// Возвращает идентификатор объекта.
        /// Должен быть переопределен в класса-потомке и, для сущностей из БД, привязан к целочисленному свойству-идентификатору.
        /// </summary>
        public abstract int ID
        {
            get;
            set;
        }

        /// <summary>
        /// Получает или возвращает контекст, выполняющий создание или присоединение экземпляра объекта.
        /// </summary>
        internal Data.DbContextBase DbContext
        {
            get;
            private set;
        }

        /// <summary>
        /// Возвращает коллекцию свойств объекта метаданных.
        /// </summary>
        [NotMapped]
        public MetadataObjectPropertyCollection PropertyCollection
        {
            get => _propertyCollection.Value;
        }

        /// <summary>
        /// Данные свойств текущего объекта.
        /// </summary>
        internal List<Db.MetadataObjectPropertyData> SourcePropertyData
        {
            get;
            set;
        }
        #endregion
    }
}
