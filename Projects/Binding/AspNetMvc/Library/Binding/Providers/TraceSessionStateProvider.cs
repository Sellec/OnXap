using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;

namespace OnXap.Binding.Providers
{
    using Core.Db;

    static class TraceSessionStorage
    {
        const int _timeoutSave = 30000;

        private static object _saveTaskSyncRoot = new object();
        private static Task _saveTask = null;

        /// <summary>
        /// Кеш всех записей сессий.
        /// </summary>
        private static ConcurrentDictionary<string, UserSession> _sessionsCache = null;

        /// <summary>
        /// Основной контекст, хранящий записи и предоставляющий возможность их сохранять при изменениях. Чтение новых записей осуществляется при помощи локальных контекстов.
        /// </summary>
        private static Lazy<Session.SessionContext> _sessionsSaveContext = null;

        /// <summary>
        /// Нужен для обеспечения исключительного доступа к контексту <see cref="_sessionsSaveContext"/> и кешу всех записей сессий <see cref="_sessionsCache"/>.
        /// </summary>
        private static object _sessionsSyncRoot = new object();

        private static ConcurrentDictionary<string, UserSession> _upsertQueue { get; set; }
        private static ConcurrentDictionary<string, string> _deleteQueue { get; set; }

        static TraceSessionStorage()
        {
            _sessionsSaveContext = new Lazy<Session.SessionContext>(() => new Session.SessionContext());
            _upsertQueue = new ConcurrentDictionary<string, UserSession>();
            _deleteQueue = new ConcurrentDictionary<string, string>();
        }

        /// <summary>
        /// Возвращает кеш сессий.
        /// </summary>
        /// <returns></returns>
        public static ConcurrentDictionary<string, UserSession> GetSessionsCache()
        {
            lock (_sessionsSyncRoot)
            {
                if (_sessionsCache == null)
                {
                    _sessionsCache = new ConcurrentDictionary<string, UserSession>();
                    var value = _sessionsSaveContext.Value;
                    TaskReadFromDB();
                }

                return _sessionsCache;
            }
        }

        private static void TaskReadFromDB()
        {
            try
            {
                if (!_sessionsSaveContext.IsValueCreated)
                    return;

                lock (_sessionsSyncRoot)
                {
                    using (var db = new Session.SessionContext())
                    {
                        var dateTimeNow = DateTime.UtcNow;
                        foreach (var res in db.Sessions.AsNoTracking().Where(x => x.Expires > dateTimeNow)) _sessionsCache.TryAdd(res.SessionId, res);
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine("TaskReadFromDB.Error: {0}", ex.Message); }
        }

        public static void AddSession(UserSession sessionItem)
        {
            if (GetSessionsCache().TryAdd(sessionItem.SessionId, sessionItem))
            {
                _upsertQueue.TryAdd(sessionItem.SessionId, sessionItem);
                SaveChanges();
            }
        }

        public static void MarkSessionUpdated(UserSession sessionItem)
        {
            var sessionItem2 = GetSessionsCache().GetOrAdd(sessionItem.SessionId, sessionItem);
            _upsertQueue.TryAdd(sessionItem.SessionId, sessionItem);
            SaveChanges();
        }

        public static void RemoveSession(UserSession sessionItem)
        {
            if (sessionItem == null) return;

            if (GetSessionsCache().TryRemove(sessionItem.SessionId, out sessionItem))
            {
                _deleteQueue.TryAdd(sessionItem.SessionId, sessionItem.SessionId);
                SaveChanges();
            }
        }

        public static UserSession GetSessionItem(string id)
        {
            UserSession item = null;
            if (GetSessionsCache().TryGetValue(id, out item))
            {
                if (item.Expires < DateTime.UtcNow)
                {
                    // Сессия найдена, но она истекла
                    _deleteQueue.TryAdd(id, id);
                    SaveChanges();
                    return null;
                }
                return item;
            }
            else
            {
                return null;
            }
        }

        public static void SaveChanges()
        {
            lock (_saveTaskSyncRoot)
            {
                if (_saveTask == null)
                    _saveTask = Task.Delay(_timeoutSave).ContinueWith(t =>
                    {
                        try
                        {
                            var oldUpsertQueue = _upsertQueue.Values.ToList();
                            _upsertQueue = new ConcurrentDictionary<string, UserSession>();

                            var oldDeleteQueue = _deleteQueue.Values.ToList();
                            _deleteQueue = new ConcurrentDictionary<string, string>();

                            if (oldUpsertQueue.Count > 0)
                            {
                                _sessionsSaveContext.Value.Sessions.
                                    UpsertRange(oldUpsertQueue).
                                    On(x => x.SessionId).
                                    WhenMatched((xDb, xIns) => new UserSession()
                                    {
                                        Created = xIns.Created,
                                        Expires = xIns.Expires,
                                        LockDate = xIns.LockDate,
                                        LockId = xIns.LockId,
                                        Locked = xIns.Locked,
                                        ItemContent = xIns.ItemContent,
                                        IdUser = xIns.IdUser
                                    }).
                                    Run();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("SessionStateProvider: Update error: {0}", ex.Message);
                        }
                        finally
                        {
                            lock (_saveTaskSyncRoot) _saveTask = null;
                        }
                    });
            }
        }
    }

    /// <summary>
    /// Наш собственный провайдер.
    /// </summary>
    class TraceSessionStateProvider : SessionStateStoreProviderBase
    {
        int _timeoutSession = 0;

        private class InternalStoreInfo
        {
            public const string KEY = "SECRET_SESSION_ITEM_DOES_NOT_REMOVE";

            public SessionStateStoreData item;
            public string id;
            public object lockId;
            public TraceSessionStateProvider provider;
        }

        /// <summary>
        /// Инициализация провайдера, читаем конфигурацию, устаналиваем переменные...
        /// </summary>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null) throw new ArgumentNullException("config");
            base.Initialize(name, config);

            var applicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            var configuration = WebConfigurationManager.OpenWebConfiguration(applicationName);

            var configSection = (SessionStateSection)configuration.GetSection("system.web/sessionState");
            _timeoutSession = 60 * 24 * 365;// (int)configSection.Timeout.TotalMinutes;
        }

        #region Методы SessionStateStoreProviderBase 
        public override void Dispose()
        {
            //_dataContext.Dispose();
        }

        /// <summary>
        /// Получаем сессию для режима "только для чтения" без необходимости блокировки.
        /// </summary>
        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            var sessionItem = GetSessionItem(context, id, false, out locked, out lockAge, out lockId, out actions);
            context.Items[InternalStoreInfo.KEY] = new InternalStoreInfo() { id = id, lockId = lockId, item = sessionItem, provider = this };
            return sessionItem;
        }

        /// <summary>
        /// Получаем сессию в режиме эксклюзивного доступа с необходимостью блокировки.
        /// </summary>
        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            return GetSessionItem(context, id, true, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// Обобщенный вспомогательный метод для получения доступа к сессии в базе данных.
        /// Используется как GetItem, так и GetItemExclusive.
        /// </summary>
        private SessionStateStoreData GetSessionItem(HttpContext context, string id, bool exclusive, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            locked = false;
            lockAge = new TimeSpan();
            lockId = null;
            actions = 0;

            var sessionItem = TraceSessionStorage.GetSessionItem(id);

            // Сессия не найдена
            if (sessionItem == null) return null;

            // Сессия найдена, но заблокирована
            if (sessionItem.Locked)
            {
                locked = true;
                lockAge = DateTime.UtcNow - sessionItem.LockDate;
                lockId = sessionItem.LockId;
                return null;
            }

            // Сессия найдена, требуется эксклюзинвый доступ.
            if (exclusive)
            {
                lock (sessionItem.SyncRoot)
                {
                    sessionItem.LockId += 1;
                    sessionItem.Locked = true;
                    sessionItem.LockDate = DateTime.UtcNow;
                    sessionItem.DateLastChanged = DateTime.Now;
                    TraceSessionStorage.MarkSessionUpdated(sessionItem);
                }
            }

            locked = exclusive;
            lockAge = DateTime.UtcNow - sessionItem.LockDate;
            lockId = sessionItem.LockId;

            var data = (sessionItem.ItemContent == null)
                ? CreateNewStoreData(context, _timeoutSession)
                : Deserialize(context, sessionItem.ItemContent, _timeoutSession);

            data.Items["UserId"] = sessionItem.IdUser;

            return data;
        }

        /// <summary>
        /// Удаляем блокировку сессии, освобождаем ее для других потоков.
        /// </summary>
        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            var sessionItem = TraceSessionStorage.GetSessionItem(id);
            if (sessionItem.LockId != (int)lockId) return;

            lock (sessionItem.SyncRoot)
            {
                sessionItem.Locked = false;
                sessionItem.Expires = DateTime.UtcNow.AddMinutes(_timeoutSession);
                sessionItem.DateLastChanged = DateTime.Now;
                TraceSessionStorage.MarkSessionUpdated(sessionItem);
            }
        }

        /// <summary>
        /// Сохраняем состояние сессии и снимаем блокировку.
        /// </summary>
        public override void SetAndReleaseItemExclusive(HttpContext context,
                                                        string id,
                                                        SessionStateStoreData item,
                                                        object lockId,
                                                        bool newItem)
        {
            context.Items[InternalStoreInfo.KEY] = null;

            var intLockId = lockId == null ? 0 : (int)lockId;
            var userId = (int)item.Items["UserId"];

            var data = ((SessionStateItemCollection)item.Items);
            data.Remove("UserId");

            // Сериализуем переменные
            var itemContent = Serialize(data);

            // Если это новая сессия, которой еще нет в базе данных.
            if (newItem)
            {
                CreateUninitializedItem(context, id, _timeoutSession);
                //return;//Не выходим из функции, т.к. в новую сессию еще надо записать данные.
            }

            // Если это старая сессия, проверяем совпадает ли ключ блокировки, 
            // а после сохраняем состояние и снимаем блокировку.
            var sessionItem = TraceSessionStorage.GetSessionItem(id);
            if (sessionItem != null)
            {
                lock (sessionItem.SyncRoot)
                {
                    if (lockId == null || sessionItem.LockId == (int)lockId)
                    {
                        sessionItem.IdUser = userId;
                        sessionItem.ItemContent = itemContent;
                        sessionItem.Expires = DateTime.UtcNow.AddMinutes(_timeoutSession);
                        sessionItem.Locked = false;
                        sessionItem.DateLastChanged = DateTime.Now;
                        TraceSessionStorage.MarkSessionUpdated(sessionItem);
                    }
                }
            }
        }

        public static void SaveUnsavedSessionItem()
        {
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Items.Contains(InternalStoreInfo.KEY))
                {
                    var sessionInfo = HttpContext.Current.Items[InternalStoreInfo.KEY] as InternalStoreInfo;
                    if (sessionInfo != null && sessionInfo.item != null && sessionInfo.item.Items != null)
                    {
                        if (sessionInfo.item.Items.Dirty)
                        {
                            sessionInfo.provider.SetAndReleaseItemExclusive(HttpContext.Current, sessionInfo.id, sessionInfo.item, sessionInfo.lockId, false);
                        }
                    }

                    HttpContext.Current.Items.Remove(InternalStoreInfo.KEY);
                }
            }

        }

        /// <summary>
        /// Удаляет запись о состоянии сессии.
        /// </summary>
        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            var sessionItem = TraceSessionStorage.GetSessionItem(id);
            lock (sessionItem.SyncRoot)
            {
                if (sessionItem.LockId != (int)lockId) return;

                sessionItem.DateLastChanged = DateTime.Now;
                TraceSessionStorage.RemoveSession(sessionItem);
            }
        }

        /// <summary>
        /// Сбрасывает счетчик жизни сессии.
        /// </summary>
        public override void ResetItemTimeout(HttpContext context, string id)
        {
            var sessionItem = TraceSessionStorage.GetSessionItem(id);
            if (sessionItem == null) return;

            lock (sessionItem.SyncRoot)
            {
                sessionItem.Expires = DateTime.UtcNow.AddMinutes(_timeoutSession);
                sessionItem.DateLastChanged = DateTime.Now;
                TraceSessionStorage.MarkSessionUpdated(sessionItem);
            }
        }

        /// <summary>
        /// Создается новый объект, который будет использоваться для хранения состояния сессии в течении запроса.
        /// Мы можем установить в него некоторые предопределенные значения, которые нам понадобятся.
        /// </summary>
        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            var data = new SessionStateStoreData(new SessionStateItemCollection(),
                                                    SessionStateUtility.GetSessionStaticObjects(context),
                                                    timeout);

            if (data.Items["UserId"] == null) data.Items["UserId"] = 0;
            return data;
        }

        /// <summary>
        /// Создание пустой записи о новой сессии в хранилище сессий.
        /// </summary>
        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            var session = new UserSession
            {
                SessionId = id,
                IdUser = 0,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(timeout),
                LockDate = DateTime.UtcNow,
                Locked = false,
                ItemContent = null,
                LockId = 0,
            };

            TraceSessionStorage.AddSession(session);
        }

        #endregion

        #region Ненужные методы в данной реализации

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback) { return false; }
        public override void EndRequest(HttpContext context) { }
        public override void InitializeRequest(HttpContext context) { }

        #endregion

        #region Вспомогательные методы сериализации и десериализации

        private byte[] Serialize(SessionStateItemCollection items)
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);

            if (items != null) items.Serialize(writer);
            writer.Close();

            return ms.ToArray();
        }

        private SessionStateStoreData Deserialize(HttpContext context, Byte[] serializedItems, int timeout)
        {
            var ms = new MemoryStream(serializedItems);

            var sessionItems = new SessionStateItemCollection();

            if (ms.Length > 0)
            {
                var reader = new BinaryReader(ms);
                sessionItems = SessionStateItemCollection.Deserialize(reader);
            }

            return new SessionStateStoreData(sessionItems, SessionStateUtility.GetSessionStaticObjects(context), timeout);
        }

        #endregion
    }

    namespace Session
    {
        using Core.Db;

        class SessionContext : CoreContextBase
        {
            public DbSet<UserSession> Sessions { get; set; }
        }
    }
}