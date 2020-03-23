using OnUtils.Architecture.AppCore;
using OnUtils.Data;
using OnUtils.Data.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnXap.Modules.ItemsCustomize
{
    using Core.Items;
    using Core.Modules;
    using DB;
    using Scheme;

#pragma warning disable CS1591 // todo внести комментарии.
    [ModuleCore("Настройка объектов")]
    public class ModuleItemsCustomize : ModuleCore<ModuleItemsCustomize>, IUnitOfWorkAccessor<Context>, ICritical
    {
        public class CacheCollection
        {
            private ModuleItemsCustomize _module = null;
            private IList<CustomFieldsScheme> _schemes = null;
            private Dictionary<string, DefaultScheme> _dictionary = new Dictionary<string, DefaultScheme>();

            internal CacheCollection(ModuleItemsCustomize module)
            {
                using (var db = module.CreateUnitOfWork())
                {
                    _module = module;
                    _schemes = db.CustomFieldsSchemes.Where(x => x.IdScheme > 0).ToList();
                }
            }

            private DefaultScheme CreateDefaultScheme()
            {
                var def = new DefaultScheme();

                foreach (var scheme in _schemes)
                {
                    def.Schemes[(uint)scheme.IdScheme] = new Scheme.Scheme(def)
                    {
                        IdScheme = (uint)scheme.IdScheme,
                        NameScheme = scheme.NameScheme
                    };
                }

                return def;
            }

            public int Count
            {
                get => _dictionary.Count;
            }

            public DefaultScheme this[SchemeItem schemeItem]
            {
                get => this[SchemeItem.GetEqualityKey(schemeItem)];
            }

            internal DefaultScheme this[string schemeItemKey]
            {
                get
                {
                    if (!_dictionary.ContainsKey(schemeItemKey)) _dictionary[schemeItemKey] = CreateDefaultScheme();
                    return _dictionary[schemeItemKey];
                }
            }
        }

        public static readonly Guid PERM_EXTFIELDS_ALLOWMANAGE = "PERM_EXTFIELDS_ALLOWMANAGE".GenerateGuid();
        internal static ModuleItemsCustomize _moduleLink = null;

        private object SyncRoot = new object();
        private CacheCollection _cache = null;

        public ModuleItemsCustomize()
        {
        }

        protected override void OnModuleStarting()
        {
            RegisterPermission(PERM_EXTFIELDS_ALLOWMANAGE, "Настройка схемы полей");

            Task.Delay(60000).ContinueWith(t => TimerCallback());

            _moduleLink = this;
        }

        protected internal override void OnModuleStop()
        {
            if (_moduleLink == this) _moduleLink = null;
        }

        #region Property
        protected CacheCollection Cache
        {
            get
            {
                lock (SyncRoot)
                {
                    if (_cache == null) CreateCache();
                    return _cache;
                }
            }
        }

        protected bool AllowSchemeManage
        {
            get => CheckPermission(PERM_EXTFIELDS_ALLOWMANAGE) == CheckPermissionResult.Allowed;
        }
        #endregion

        #region Cache
        private void CreateCache()
        {
            var measure = new MeasureTime();
            Exception exception = null;

            try
            {
                lock (SyncRoot)
                {
                    _cache = null;

                    using (var db = this.CreateUnitOfWork())
                    {
                        var q_items = (from ip in db.ItemParent
                                       group ip by new { ip.IdItem, ip.IdItemType } into grp
                                       select grp.Key);

                        var q_fields = db.CustomFieldsFields.Where(x => x.Block == 0).Include(x => x.data).ToList();

                        var items = from p in db.ItemParent
                                    join ip in q_items on new { p.IdItem, p.IdItemType } equals new { ip.IdItem, ip.IdItemType }
                                    join sd in db.CustomFieldsSchemeDatas on new { p.IdItemType, IdItem = p.IdParentItem } equals new { sd.IdItemType, IdItem = sd.IdSchemeItem }
                                    join sch in db.CustomFieldsSchemes on sd.IdScheme equals sch.IdScheme into joinedScheme
                                    from sch in joinedScheme.DefaultIfEmpty()
                                    join fi in db.CustomFieldsFields on sd.IdField equals fi.IdField
                                    where fi.Block == 0
                                    orderby p.IdItemType ascending, p.IdItem ascending, p.IdLevel ascending, sd.IdScheme ascending, sd.Order ascending
                                    select new
                                    {
                                        ItemParent = p,
                                        SchemeData = sd,
                                        Scheme = sch,
                                        Field = fi
                                    };

                        var fields = new Dictionary<int, CustomFieldsField>();
                        var cache = new CacheCollection(this);
                        foreach (var row in items)
                        {
                            if (row.SchemeData.IdScheme > 0 && row.Scheme == null) continue;

                            var key = SchemeItem.GetEqualityKey(row.ItemParent.IdItemType, row.ItemParent.IdItem);
                            if (!fields.ContainsKey(row.Field.IdField)) fields.Add(row.Field.IdField, row.Field);

                            var defaultScheme = cache[SchemeItem.GetEqualityKey(row.ItemParent.IdItemType, row.ItemParent.IdItem)];
                            var field = fields[row.Field.IdField];

                            /*
                             * Хак на время запуска - схемы 0 может и не существовать. На всякий случай собираем все поля для схемы Default из остальных схем.
                             * */
                            defaultScheme[field.IdField] = field;
                            if (field.data != null) field.data = field.FieldType.CreateValuesCollection(field, field.data);

                            if (row.SchemeData.IdScheme == 0) defaultScheme[field.IdField] = field;
                            else
                            {
                                if (!defaultScheme.Schemes.ContainsKey((uint)row.SchemeData.IdScheme))
                                {
                                    var sch = new Scheme.Scheme(defaultScheme)
                                    {
                                        IdScheme = (uint)row.Scheme.IdScheme,
                                        NameScheme = row.Scheme.NameScheme
                                    };
                                    defaultScheme.Schemes.Add((uint)row.SchemeData.IdScheme, sch);
                                }
                                (defaultScheme.Schemes[(uint)row.SchemeData.IdScheme] as Scheme.Scheme)[field.IdField] = field;
                            }
                        }

                        _cache = cache;
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                if (exception == null && measure.Calculate(false).TotalMilliseconds >= 50)
                    Debug.WriteLineNoLog("CreateCache: {0} items, {1}ms", _cache != null ? _cache.Count : 0, measure);
                else if (exception != null)
                    Debug.WriteLine("CreateCacheFailed: {0}", exception.GetLowLevelException().Message);
            }
        }

        internal void ClearCache(bool norecursive = false)
        {
            lock (SyncRoot)
            {
                _cache = null;
                Proxy.ProxyHelper.ClearCache();
            }
        }

        public void UpdateCache()
        {
            lock (SyncRoot)
            {
                ClearCache();
                CreateCache();
            }
        }
        #endregion

        #region Deffered
        #region Clear links
        private class TimerData
        {
            public DateTime DateCreated { get; } = DateTime.Now;

            public Dictionary<ItemBase, Type> Objects = new Dictionary<ItemBase, Type>();
        }

        private ConcurrentQueue<TimerData> _linksListsQueue = new ConcurrentQueue<TimerData>();

        private TimerData TimerClearDeffered
        {
            get
            {
                TimerData data;
                if (!_linksListsQueue.IsEmpty)
                {
                    var last = _linksListsQueue.Last();
                    if ((DateTime.Now - last.DateCreated).TotalMinutes >= 1)
                    {
                        data = new TimerData();
                        _linksListsQueue.Enqueue(data);
                        return data;
                    }
                    else return last;
                }
                else
                {
                    data = new TimerData();
                    _linksListsQueue.Enqueue(data);
                    return data;
                }
            }
        }

        private void TimerCallback()
        {
            try
            {
                TimerData dataPeek;
                TimerData dataDequeued;
                while (!_linksListsQueue.IsEmpty)
                {
                    if (_linksListsQueue.TryPeek(out dataPeek))
                    {
                        if ((DateTime.Now - dataPeek.DateCreated).TotalMinutes >= 1)
                        {
                            if (_linksListsQueue.TryDequeue(out dataDequeued) && object.ReferenceEquals(dataDequeued, dataPeek))
                            {
                                if (dataDequeued.Objects.Count > 0)
                                {
                                    var objectsGroupedByType = dataDequeued.Objects.Where(x => x.Key != null && x.Value != null).GroupBy(x => x.Value).ToDictionary(x => x.Key, x => x.ToList());
                                    foreach (var pair in objectsGroupedByType)
                                    {
                                        var objType = pair.Key;
                                        ConcurrentBag<ItemBase> list = null;
                                        if (_defferedObjects.TryGetValue(objType, out list))
                                        {
                                            ItemBase item;
                                            foreach (var obj in pair.Value)
                                            {
                                                list.TryTake(out item);
                                            }
                                        }
                                    }
                                    dataDequeued.Objects.Clear();
                                }
                            }
                        }
                        else break;
                    }
                    else break;
                }
            }
            finally
            {
                Task.Delay(60000).ContinueWith(t => TimerCallback());
            }
        }
        #endregion

        private ConcurrentDictionary<Type, ConcurrentBag<ItemBase>> _defferedObjects = new ConcurrentDictionary<Type, ConcurrentBag<ItemBase>>();

        public void RegisterToQuery<TItem>(TItem obj) where TItem : ItemBase
        {
            var objType = obj.GetType();
            var list = _defferedObjects.GetOrAdd(objType, k => new ConcurrentBag<ItemBase>());

            if (!list.Contains(obj)) list.Add(obj);

            TimerClearDeffered.Objects[obj] = objType;
        }

        internal void CheckDeffered(Type type = null)
        {
            foreach (var pair in _defferedObjects)
            {
                if (type != null & pair.Key != type) continue;

                var items = pair.Value.ToList();

                int start = 0;
                List<ItemBase> subItems = null;

                do
                {
                    subItems = items.Skip(start).Take(500).ToList();
                    start += 500;
                    if (subItems.Count() > 0)
                    {
                        var fieldsForItems = GetItemsFields(subItems);
                        if (fieldsForItems != null)
                        {
                            foreach (var p in fieldsForItems)
                            {
                                p.Key._fields = p.Value;
                            }

                            foreach (var p in subItems)
                            {
                                p._fields = p.FieldsBase;
                            }
                        }
                    }
                    else break;
                } while (subItems.Count() > 0);

                ItemBase someItem;
                while (!pair.Value.IsEmpty) pair.Value.TryTake(out someItem);
            }
        }
        #endregion

        private ConcurrentDictionary<object, Data.DefaultSchemeWData> _itemsData = new ConcurrentDictionary<object, Data.DefaultSchemeWData>();

        public IDictionary<TItem, Data.DefaultSchemeWData> GetItemsFields<TItem>(IEnumerable<TItem> items) where TItem : ItemBase
        {
            var measure = new MeasureTime();
            var measure2 = new MeasureTime();
            TimeSpan spanIterate = TimeSpan.Zero;
            TimeSpan spanQuery = TimeSpan.Zero;

            try
            {
                var collection = new Dictionary<TItem, Data.DefaultSchemeWData>();
                var items2 = new Dictionary<int, List<TItem>>();
                var ids = new List<int>();

                lock (SyncRoot)
                {
                    var ca = Cache;
                }

                int? IdItemTypeFirst = null;
                foreach (var item in items)
                {
                    var schemeItem = SchemeItemAttribute.GetValueFromObject(item);
                    var IdItemType = ItemTypeAttribute.GetValueFromObject(item);

                    if (IdItemTypeFirst == null) IdItemTypeFirst = IdItemType;
                    else if (IdItemTypeFirst.Value != IdItemType) throw new NotSupportedException("Список items должен содержать элементы с одинаковым IdItemType.");

                    var defaultSchemeToItem = Cache[schemeItem];

                    var fieldsData = Proxy.ProxyHelper.CreateTypeObjectFromParent<Data.DefaultSchemeWData, Field.IField>(defaultSchemeToItem);
                    fieldsData._schemeItemSource = schemeItem;
                    //var schemes = new Dictionary<int, Data.SchemeWData>();
                    //foreach (var scheme in fieldsData.Default.Schemes)
                    //{
                    //    var scheme = 
                    //}
                    //fieldsData.Schemes = new System.Collections.ObjectModel.ReadOnlyDictionary<int, Data.SchemeWData>(schemes);

                    var id = item.ID;
                    collection[item] = fieldsData;
                    if (!items2.ContainsKey(id)) items2[id] = new List<TItem>();
                    items2[id].AddIfNotExists(item);
                    if (!ids.Contains(id)) ids.Add(id);
                }
                spanIterate = measure2.Calculate();

                if (collection.Count > 0)
                {
                    using (var db = this.CreateUnitOfWork())
                    {
                        var _ids = ids.ToArray();
                        var values = (from p in db.CustomFieldsDatas.AsNoTracking()
                                      join fi in db.CustomFieldsFields.AsNoTracking() on p.IdField equals fi.IdField
                                      where ids.Contains(p.IdItem) && p.IdItemType == IdItemTypeFirst.Value && fi.Block == 0
                                      select p);

                        foreach (var res in values)
                        {
                            items2[res.IdItem].ForEach(item =>
                            {
                                var field = collection[item][res.IdField];
                                if (field != null) field.AddValue(res);
                            });
                        }
                    }
                }
                spanQuery = measure2.Calculate();

                return collection;
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.Error, "Ошибка получения полей", $"{items?.Count()} items, {measure}ms all", ex);
                return null;
            }
        }

        public Data.DefaultSchemeWData GetItemFields<TItem>(TItem item) where TItem : ItemBase
        {
            var fields = GetItemsFields(new List<TItem>() { item });
            return fields?.Select(x=>x.Value)?.FirstOrDefault();
        }

        public IScheme<Field.IField> getSchemeFullByItem2(int itemID, int itemType, uint? schemeID = null)
        {
            try
            {
                var defaultScheme = Cache[SchemeItem.GetEqualityKey(itemType, itemID)];

                if (!schemeID.HasValue || schemeID.Value == 0) return defaultScheme;
                if (defaultScheme.Schemes.ContainsKey(schemeID.Value)) return defaultScheme.Schemes[schemeID.Value];
            }
            catch (Exception ex)
            {
                // todo setError(ex.Message);
            }
            return null;
        }

        #region Проверка и сохранение данных
        public void SaveItemFields<TItem>(TItem item) where TItem : ItemBase
        {
            if (item._fields != null)
            {
                var IdItemType = ItemTypeAttribute.GetValueFromObject(item);

                //using (var scope = DB.CreateScope())
                using (var db = new UnitOfWork<CustomFieldsData, CustomFieldsField>())
                using (var scope = db.CreateScope())
                {
                    (from d in db.Repo1
                     join f in db.Repo2 on d.IdField equals f.IdField
                     where d.IdItem == item.ID && d.IdItemType == IdItemType
                     select d).Delete();

                    foreach (var field in item._fields.Values)
                    {
                        if (field._values != null)
                            foreach (var value in field._values)
                            {
                                int IdFieldValue = 0;
                                if (!int.TryParse(value.ToString(), out IdFieldValue)) IdFieldValue = 0;

                                db.Repo1.Add(new CustomFieldsData()
                                {
                                    IdField = field.IdField,
                                    IdFieldValue = field.IdValueType == Field.FieldValueType.KeyFromSource ? IdFieldValue : 0,
                                    FieldValue = field.IdValueType == Field.FieldValueType.KeyFromSource ? "" : value.ToString(),
                                    IdItem = item.ID,
                                    IdItemType = IdItemType,
                                    DateChange = DateTime.Now.Timestamp()
                                });
                            }
                    }

                    db.SaveChanges<CustomFieldsData>();

                    scope.Commit();
                }
            }
        }

        #endregion

        /// <summary>
        /// Возвращает поле, имеющее указанный <paramref name="alias"/> (см. <see cref="Field.IField.alias"/>), для связанного модуля. Если полей с таким alias несколько, вернет первое добавленное в базу.
        /// </summary>
        /// <returns>Возвращает объект поля, либо null, если поле не найдено или произошла ошибка.</returns>
        public Field.IField GetFieldByAlias(string alias)
        {
            try
            {
                using (var db = this.CreateUnitOfWork())
                {
                    var q_fields = db.CustomFieldsFields.Where(x => x.alias == alias && x.Block == 0).Include(x => x.data);
                    return q_fields.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.Error, "Ошибка при получении поля", $"alias={alias}.", ex);
                return null;
            }
        }

        /// <summary>
        /// Возвращает поле, имеющее указанный идентификатор <paramref name="idField"/>, для связанного модуля.
        /// </summary>
        /// <returns>Возвращает объект поля, либо null, если поле не найдено или произошла ошибка.</returns>
        public Field.IField GetFieldByID(int idField)
        {
            try
            {
                using (var db = this.CreateUnitOfWork())
                {
                    var q_fields = db.CustomFieldsFields.Where(x => x.IdField == idField && x.Block == 0).Include(x => x.data);
                    return q_fields.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.Error, "Ошибка при получении поля", $"idField={idField}.", ex);
                return null;
            }
        }
    }
}
