using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OnXap.Modules.Routing
{
    using Core.Items;
    using Core.Modules;
    using DefferedDictionary = ConcurrentDictionary<Type, ConcurrentDictionary<Core.Items.ItemBase, int>>;

    class ThreadInfo
    {
        public int ThreadId { get; set; }

        public object SyncRoot { get; set; }

        public DefferedDictionary Collection { get; set; }

        public DateTime DateCreate { get; set; }

        public DateTime? DateClose { get; set; }
    }

#pragma warning disable CS1591 // todo внести комментарии.
    [ModuleCore("Маршрутизация")]
    public class ModuleRouting : ModuleCore<ModuleRouting>
    {
        internal static ModuleRouting _moduleLink = null;

        private ThreadLocal<ThreadInfo> _defferedObjects;

        public ModuleRouting()
        {
            _defferedObjects = new ThreadLocal<ThreadInfo>(() =>
             {
                 var id = Thread.CurrentThread.ManagedThreadId;
                 var oldSameIdThreads = _defferedObjects.Values.Where(x => !x.DateClose.HasValue && x.ThreadId == id).ToList();
                 if (oldSameIdThreads.Count > 0)
                 {
                     oldSameIdThreads.ForEach(pair =>
                     {
                         pair.Collection.Values.ForEach(x => x.Clear());
                         pair.Collection.Clear();
                         pair.DateClose = DateTime.Now;
                     });
                 }

                 return new ThreadInfo()
                 {
                     ThreadId = id,
                     DateCreate = DateTime.Now,
                     Collection = new DefferedDictionary(),
                     SyncRoot = new object()
                 };
             }, true);
        }

        protected override void OnModuleStarting()
        {
            _moduleLink = this;
        }

        internal protected override void OnModuleStop()
        {
            if (_moduleLink == this) _moduleLink = null;
        }

        #region Deffered
        /// <summary>
        /// Для текущего потока обрабатывает все объекты в кеше.
        /// </summary>
        public void PrepareCurrentThreadCache()
        {
            if (_defferedObjects.IsValueCreated)
            {
                var collection = _defferedObjects.Value.Collection;
                collection.ForEach(x => CheckDeffered(x.Key));
            }
        }

        /// <summary>
        /// Для текущего потока очищает кеш объектов.
        /// </summary>
        public void ClearCurrentThreadCache()
        {
            if (_defferedObjects.IsValueCreated)
            {
                var collection = _defferedObjects.Value.Collection;
                collection.Values.ForEach(x => x.Clear());
                collection.Clear();
            }
        }

        public void RegisterToQuery<TItem>(TItem obj) where TItem : ItemBase
        {
            try
            {
                var objType = obj.GetType();
                var list = _defferedObjects.Value.Collection.GetOrAdd(objType, t => new ConcurrentDictionary<ItemBase, int>());
                list[obj] = 0;
            }
            catch { }
        }

        internal void CheckDeffered(Type type)
        {
            if (_defferedObjects.IsValueCreated && _defferedObjects.Value.Collection.ContainsKey(type))
            {
                List<ItemBase> items = null;
                lock (_defferedObjects.Value.SyncRoot)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            var newCollection = new ConcurrentDictionary<ItemBase, int>();
                            ConcurrentDictionary<ItemBase, int> oldCollection = null;

                            _defferedObjects.Value.Collection.AddOrUpdate(type, newCollection, (key, old) =>
                            {
                                oldCollection = old;
                                return newCollection;
                            });

                            if (oldCollection != null)
                            {
                                items = oldCollection.Keys.ToList();
                                oldCollection.Clear();
                            }
                            break;
                        }
                        catch
                        {
                            if (i == 2) throw;
                        }
                    }
                }

                var IdItemType = ItemTypeAttribute.GetValueFromType(type);

                int start = 0;
                List<ItemBase> subItems = null;

                do
                {
                    subItems = items.Skip(start).Take(500).ToList();
                    start += 500;
                    if (subItems.Count() > 0)
                    {
                        GetForQuery(IdItemType, type, subItems);
                    }
                    else break;
                } while (subItems.Count() > 0);
            }
        }

        internal void GetForQuery(int idItemType, Type type, List<ItemBase> items)
        {
            var absoluteUrl = AppCore.ServerUrl;
            var urlManager = AppCore.Get<UrlManager>();
            var idList = items.Select(x => x.ID).Distinct().ToArray();
            var result = urlManager.GetUrl(idList, idItemType, RoutingConstants.MAINKEY);
            if (!result.IsSuccess)
            {
                result = urlManager.GetUrl(idList, idItemType, RoutingConstants.MAINKEY);
            }
            if (!result.IsSuccess)
            {
                Debug.WriteLine("ItemBase.GetForQuery({0}): {1}", idItemType, result.Message);
                throw new Exception("Ошибка получения адресов");
            }

            var itemsEmpty = new Dictionary<ModuleCore, List<ItemBase>>();

            foreach (var item in items)
            {
                if (result.Result.TryGetValue(item.ID, out string value) && !string.IsNullOrEmpty(value))
                {
                    if (Uri.TryCreate(value, UriKind.Absolute, out Uri url))
                    {
                        item._routingUrlMain = url;
                        item._routingUrlMainSourceType = UrlSourceType.Routing;
                    }
                    else if (Uri.TryCreate(absoluteUrl, value, out Uri url2))
                    {
                        item._routingUrlMain = url2;
                        item._routingUrlMainSourceType = UrlSourceType.Routing;
                    }
                }
                else if (item.OwnerModule != null)
                {
                    if (!itemsEmpty.ContainsKey(item.OwnerModule)) itemsEmpty[item.OwnerModule] = new List<ItemBase>();
                    itemsEmpty[item.OwnerModule].Add(item);
                }
            }

            if (itemsEmpty.Count > 0)
            {
                itemsEmpty.ForEach(x =>
                {
                    var generated = x.Key.GenerateLinks(x.Value);
                    if (generated != null)
                        foreach (var pair in generated)
                            if (pair.Value != null)
                            {
                                pair.Key._routingUrlMain = pair.Value;
                                pair.Key._routingUrlMainSourceType = UrlSourceType.Module;
                            }
                });
            }
        }
        #endregion

    }
}
