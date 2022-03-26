using OnUtils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace OnXap.Journaling
{
    using Core;
    using Core.Data;
    using Core.Items;
    using TaskSheduling;
    using Utils;
    using ExecutionRegisterResult = ExecutionResult<int?>;
    using ExecutionResultJournalData = ExecutionResult<Model.JournalData>;
    using ExecutionResultJournalDataList = ExecutionResult<List<Model.JournalData>>;
    using ExecutionResultJournalName = ExecutionResult<Model.JournalInfo>;
    using ExecutionResultJournalOptions = ExecutionResult<JournalOptions>;

    /// <summary>
    /// Представляет менеджер системных журналов. Позволяет создавать журналы, как привязанные к определенным типам, так и вручную, и регистрировать в них события.
    /// </summary>
    public sealed class JournalingManager :
        CoreComponentBase,
        IComponentSingleton,
        ITypedJournalComponent<JournalingManager>
    {
        internal const int EventCodeDefault = 0;
        //Список журналов, основанных на определенном типе объектов.
        private ConcurrentDictionary<Type, ExecutionResultJournalName> _typedJournalsList = new ConcurrentDictionary<Type, ExecutionResultJournalName>();
        private JournalOptions _journalOptionsDefault = new JournalOptions();
        private ConcurrentDictionary<int, JournalOptions> _journalsOptions = new ConcurrentDictionary<int, JournalOptions>();
        private TaskDescription _taskDescriptionClear = null;

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStarting()
        {
            _journalOptionsDefault = new JournalOptions();
            DatabaseAccessor = AppCore.Get<DB.JournalingManagerDatabaseAccessor>();
            RegisterJournalTyped<JournalingManager>("Менеджер журналов");
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStarted()
        {
            _taskDescriptionClear = AppCore.Get<TaskSchedulingManager>().RegisterTask(new TaskRequest()
            {
                Name = "Менеджер журналов: очистка старых записей",
                IsEnabled = true,
                UniqueKey = typeof(JournalingManager).FullName + "_ClearLastNDays",
                Schedules = new List<TaskSchedule>()
                {
                    new TaskCronSchedule(Cron.Hourly()) { IsEnabled = true }
                },
                ExecutionLambda = () => ClearLastNDays(),
                TaskOptions = TaskOptions.AllowDisabling | TaskOptions.AllowManualSchedule | TaskOptions.PreventParallelExecution
            });
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
            _taskDescriptionClear = null;
        }
        #endregion

        #region Обслуживание журналов
        private void ClearLastNDays()
        {
            try
            {
                if (_taskDescriptionClear == null) return;

                foreach (var pair in _journalsOptions)
                {
                    var lastNDaysValue = pair.Value.LimitByLastNDays ?? _journalOptionsDefault.LimitByLastNDays;
                    if (!lastNDaysValue.HasValue || lastNDaysValue.Value < 0) continue;

                    var dateLimit = DateTimeOffset.UtcNow.AddDays(-lastNDaysValue.Value);

                    using (var db = new DB.DataContext())
                    {
                        var hasRecords = true;
                        while (hasRecords)
                        {
                            for (int i = 0; i <= 2; i++)
                            {
                                try
                                {
                                    var rows = db.Journal.Where(x => x.IdJournal == pair.Key && x.DateEvent < dateLimit).OrderBy(x => x.DateEvent).Take(1000).ToList();
                                    hasRecords = rows.Count > 0;
                                    if (hasRecords)
                                    {
                                        db.Journal.RemoveRange(rows);
                                        db.SaveChanges();
                                    }
                                    break;
                                }
                                catch
                                {
                                    if (i == 2) hasRecords = false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RegisterEvent<JournalingManager>(EventType.Error, "Ошибка очистки старых записей журналов", null, exception: ex);
            }
        }
        #endregion

        #region Регистрация журналов
        /// <summary>
        /// Регистрирует новый журнал или обновляет старый по ключу <paramref name="uniqueKey"/> (если передан).
        /// </summary>
        /// <param name="idType">См. <see cref="DB.JournalNameDAO.IdJournalType"/>.</param>
        /// <param name="name">См. <see cref="DB.JournalNameDAO.Name"/>.</param>
        /// <param name="uniqueKey">См. <see cref="DB.JournalNameDAO.UniqueKey"/>.</param>
        /// <param name="journalOptions">Дополнительные параметры журнала.</param>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalName"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="name"/> представляет пустую строку или null.</exception>
        [ApiIrreversible]
        public ExecutionResultJournalName RegisterJournal(int idType, string name, string uniqueKey = null, JournalOptions journalOptions = null)
        {
            try
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

                DB.JournalNameDAO data = null;

                using (var db = new DB.DataContext())
                using (var scope = db.CreateScope(TransactionScopeOption.RequiresNew))
                {
                    if (string.IsNullOrEmpty(uniqueKey))
                    {
                        var row = new DB.JournalNameDAO()
                        {
                            IdJournalType = idType,
                            Name = name,
                            UniqueKey = uniqueKey
                        };
                        db.JournalName.Add(row);
                        db.SaveChanges();
                        data = row;
                    }
                    else
                    {
                        var row = db.JournalName.Where(x => x.UniqueKey == uniqueKey).FirstOrDefault();
                        if (row == null)
                        {
                            row = new DB.JournalNameDAO()
                            {
                                IdJournalType = idType,
                                Name = name,
                                UniqueKey = uniqueKey
                            };
                            db.JournalName.Add(row);
                            db.SaveChanges();
                            data = row;
                        }
                        else
                        {
                            bool changed = false;
                            if (row.IdJournalType != idType)
                            {
                                row.IdJournalType = idType;
                                changed = true;
                            }
                            if (row.Name != name)
                            {
                                row.Name = name;
                                changed = true;
                            }

                            if (changed) db.SaveChanges();
                            data = row;
                        }
                    }
                    scope.Complete();
                }

                _journalsOptions[data.IdJournal] = journalOptions ?? new JournalOptions();

                var info = new Model.JournalInfo();
                Model.JournalInfo.Fill(info, data);
                return new ExecutionResultJournalName(true, null, info);
            }
            catch (ArgumentNullException) { throw; }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager).FullName}.{nameof(JournalingManager.RegisterJournal)}: {ex.ToString()}");
                return new ExecutionResultJournalName(false, $"Возникла ошибка во время регистрации журнала с именем '{name}'. Смотрите информацию в системном текстовом журнале.");
            }
        }

        /// <summary>
        /// Регистрирует новый журнал или обновляет старый на основе типа <typeparamref name="TJournalTyped"/>.
        /// </summary>
        /// <param name="name">См. <see cref="DB.JournalNameDAO.Name"/>.</param>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalName"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="name"/> представляет пустую строку или null.</exception>
        [ApiIrreversible]
        public ExecutionResultJournalName RegisterJournalTyped<TJournalTyped>(string name)
        {
            return RegisterJournalTypedInternal(typeof(TJournalTyped), name, null);
        }

        /// <summary>
        /// Регистрирует новый журнал или обновляет старый на основе типа <typeparamref name="TJournalTyped"/>.
        /// </summary>
        /// <param name="name">См. <see cref="DB.JournalNameDAO.Name"/>.</param>
        /// <param name="journalOptions">Дополнительные параметры журнала.</param>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalName"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="name"/> представляет пустую строку или null.</exception>
        [ApiIrreversible]
        public ExecutionResultJournalName RegisterJournalTyped<TJournalTyped>(string name, JournalOptions journalOptions)
        {
            return RegisterJournalTypedInternal(typeof(TJournalTyped), name, journalOptions);
        }

        internal ExecutionResultJournalName RegisterJournalTypedInternal(Type typedType, string name, JournalOptions journalOptions)
        {
            typedType = ManagerExtensions.GetJournalType(typedType);
            var fullName = TypeNameHelper.GetFullNameCleared(typedType);
            return RegisterJournal(JournalingConstants.IdSystemJournalType, name, JournalingConstants.TypedJournalsPrefix + fullName);
        }
        #endregion

        #region Настройки журналов
        #region Установка настроек
        /// <summary>
        /// Устанавливает свойства всех журналов по-умолчанию, если для журналов не заданы собственные значения свойств.
        /// </summary>
        /// <param name="journalOptions">Параметры журналов по-умолчанию.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="journalOptions"/> равен null.</exception>
        public void SetJournalOptionsDefault(JournalOptions journalOptions)
        {
            if (journalOptions == null) throw new ArgumentNullException(nameof(journalOptions));
            _journalOptionsDefault = journalOptions;
        }

        /// <summary>
        /// Устанавливает свойства журнала.
        /// </summary>
        /// <param name="idJournal">Идентификатор журнала. См. <see cref="GetJournal(int)"/>.</param>
        /// <param name="journalOptions">Параметры журнала.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="journalOptions"/> равен null.</exception>
        /// <returns>Возвращает объект <see cref="ExecutionResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        public ExecutionResult SetJournalOptions(int idJournal, JournalOptions journalOptions)
        {
            var journalResult = GetJournal(idJournal);
            if (!journalResult.IsSuccess) return new ExecutionResult(false, journalResult.Message);
            return SetJournalOptionsInternal(journalResult.Result.IdJournal, journalOptions);
        }

        /// <summary>
        /// Устанавливает свойства журнала.
        /// </summary>
        /// <param name="uniqueKey">Уникальный ключ журнала. См. <see cref="GetJournal(string)"/>.</param>
        /// <param name="journalOptions">Параметры журнала.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="journalOptions"/> равен null.</exception>
        /// <returns>Возвращает объект <see cref="ExecutionResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        public ExecutionResult SetJournalOptions(string uniqueKey, JournalOptions journalOptions)
        {
            var journalResult = GetJournal(uniqueKey);
            if (!journalResult.IsSuccess) return new ExecutionResult(false, journalResult.Message);
            return SetJournalOptionsInternal(journalResult.Result.IdJournal, journalOptions);
        }

        /// <summary>
        /// Устанавливает свойства журнала.
        /// </summary>
        /// <typeparam name="TJournalTyped">Тип для определения журнала. См. <see cref="GetJournalTyped{TJournalTyped}"/>.</typeparam>
        /// <param name="journalOptions">Параметры журнала.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="journalOptions"/> равен null.</exception>
        /// <returns>Возвращает объект <see cref="ExecutionResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        public ExecutionResult SetJournalOptions<TJournalTyped>(JournalOptions journalOptions)
        {
            var journalResult = GetJournalTyped<TJournalTyped>();
            if (!journalResult.IsSuccess) return new ExecutionResult(false, journalResult.Message);
            return SetJournalOptionsInternal(journalResult.Result.IdJournal, journalOptions);
        }

        /// <summary>
        /// Устанавливает свойства журнала.
        /// </summary>
        /// <param name="journalType">Тип для определения журнала. См. <see cref="GetJournalTyped(Type)"/>.</param>
        /// <param name="journalOptions">Параметры журнала.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="journalOptions"/> равен null.</exception>
        /// <returns>Возвращает объект <see cref="ExecutionResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        public ExecutionResult SetJournalOptions(Type journalType, JournalOptions journalOptions)
        {
            var journalResult = GetJournalTyped(journalType);
            if (!journalResult.IsSuccess) return new ExecutionResult(false, journalResult.Message);
            return SetJournalOptionsInternal(journalResult.Result.IdJournal, journalOptions);
        }

        internal ExecutionResult SetJournalOptionsInternal(int idJournal, JournalOptions journalOptions)
        {
            if (journalOptions == null) throw new ArgumentNullException(nameof(journalOptions));
            _journalsOptions[idJournal] = journalOptions;
            return new ExecutionResult(true);
        }
        #endregion

        #region Получение настроек
        /// <summary>
        /// Возвращает свойства всех журналов по-умолчанию.
        /// </summary>
        public JournalOptions GetJournalOptionsDefault()
        {
            return _journalOptionsDefault ?? new JournalOptions();
        }

        /// <summary>
        /// Возвращает свойства журнала.
        /// </summary>
        /// <param name="idJournal">Идентификатор журнала. См. <see cref="GetJournal(int)"/>.</param>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalOptions"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        public ExecutionResultJournalOptions GetJournalOptions(int idJournal)
        {
            var journalResult = GetJournal(idJournal);
            if (!journalResult.IsSuccess) return new ExecutionResultJournalOptions(false, journalResult.Message);
            return GetJournalOptionsInternal(journalResult.Result.IdJournal);
        }

        /// <summary>
        /// Возвращает свойства журнала.
        /// </summary>
        /// <param name="uniqueKey">Уникальный ключ журнала. См. <see cref="GetJournal(string)"/>.</param>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalOptions"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        public ExecutionResultJournalOptions GetJournalOptions(string uniqueKey)
        {
            var journalResult = GetJournal(uniqueKey);
            if (!journalResult.IsSuccess) return new ExecutionResultJournalOptions(false, journalResult.Message);
            return GetJournalOptionsInternal(journalResult.Result.IdJournal);
        }

        /// <summary>
        /// Возвращает свойства журнала.
        /// </summary>
        /// <typeparam name="TJournalTyped">Тип для определения журнала. См. <see cref="GetJournalTyped{TJournalTyped}"/>.</typeparam>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalOptions"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        public ExecutionResultJournalOptions GetJournalOptions<TJournalTyped>()
        {
            var journalResult = GetJournalTyped<TJournalTyped>();
            if (!journalResult.IsSuccess) return new ExecutionResultJournalOptions(false, journalResult.Message);
            return GetJournalOptionsInternal(journalResult.Result.IdJournal);
        }

        /// <summary>
        /// Возвращает свойства журнала.
        /// </summary>
        /// <param name="journalType">Тип для определения журнала. См. <see cref="GetJournalTyped(Type)"/>.</param>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalOptions"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        public ExecutionResultJournalOptions GetJournalOptions(Type journalType)
        {
            var journalResult = GetJournalTyped(journalType);
            if (!journalResult.IsSuccess) return new ExecutionResultJournalOptions(false, journalResult.Message);
            return GetJournalOptionsInternal(journalResult.Result.IdJournal);
        }

        internal ExecutionResultJournalOptions GetJournalOptionsInternal(int idJournal)
        {
            return new ExecutionResultJournalOptions(true, null, _journalsOptions.TryGetValue(idJournal, out var journalOptions) ? journalOptions : new JournalOptions());
        }
        #endregion
        #endregion

        #region Получить журналы
        /// <summary>
        /// Возвращает журнал по уникальному ключу <paramref name="uniqueKey"/>.
        /// </summary>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalName"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="uniqueKey"/> представляет пустую строку или null.</exception>
        [ApiIrreversible]
        public ExecutionResultJournalName GetJournal(string uniqueKey)
        {
            try
            {
                if (string.IsNullOrEmpty(uniqueKey)) throw new ArgumentNullException(nameof(uniqueKey));

                using (var db = new DB.DataContext())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress))
                {
                    var data = db.JournalName.Where(x => x.UniqueKey == uniqueKey).FirstOrDefault();
                    if (data == null) return new ExecutionResultJournalName(false, "Журнал с указанным уникальным ключом не найден.");

                    var info = new Model.JournalInfo();
                    Model.JournalInfo.Fill(info, data);
                    return new ExecutionResultJournalName(true, null, info);
                }
            }
            catch (ArgumentNullException) { throw; }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager).FullName}.{nameof(JournalingManager.GetJournal)}(string): {ex.ToString()}");
                return new ExecutionResultJournalName(false, $"Возникла ошибка во время получения журнала с уникальным именем '{uniqueKey}'. Смотрите информацию в системном текстовом журнале.");
            }
        }

        /// <summary>
        /// Возвращает журнал по идентификатору <paramref name="IdJournal"/>.
        /// </summary>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalName"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        [ApiIrreversible]
        public ExecutionResultJournalName GetJournal(int IdJournal)
        {
            try
            {
                using (var db = new DB.DataContext())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress))
                {
                    var data = db.JournalName.Where(x => x.IdJournal == IdJournal).FirstOrDefault();
                    if (data == null) return new ExecutionResultJournalName(false, "Журнал с указанным уникальным идентификатором не найден.");

                    var info = new Model.JournalInfo();
                    Model.JournalInfo.Fill(info, data);
                    return new ExecutionResultJournalName(true, null, info);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager).FullName}.{nameof(JournalingManager.GetJournal)}(int): {ex.ToString()}");
                return new ExecutionResultJournalName(false, $"Возникла ошибка во время получения журнала с идентификатором '{IdJournal}'. Смотрите информацию в системном текстовом журнале.");
            }
        }

        /// <summary>
        /// Возвращает журнал на основе типа <typeparamref name="TJournalTyped"/>.
        /// </summary>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalName"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        [ApiIrreversible]
        public ExecutionResultJournalName GetJournalTyped<TJournalTyped>()
        {
            return GetJournalTyped(typeof(TJournalTyped));
        }

        internal ExecutionResultJournalName GetJournalTyped(Type typeTyped)
        {
            return _typedJournalsList.GetOrAddWithExpiration(
                typeTyped,
                (t) =>
                {
                    var fullName = TypeNameHelper.GetFullNameCleared(typeTyped);
                    return GetJournal(JournalingConstants.TypedJournalsPrefix + fullName);
                },
                TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// Возвращает события, связанные с объектом <paramref name="relatedItem"/> во всех журналах.
        /// </summary>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalDataList"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="relatedItem"/> равен null.</exception>
        [ApiIrreversible]
        public ExecutionResultJournalDataList GetJournalForItem(ItemBase relatedItem)
        {
            if (relatedItem == null) throw new ArgumentNullException(nameof(relatedItem));
            var itemType = ItemTypeFactory.GetItemType(relatedItem.GetType());
            if (itemType == null) return new ExecutionResultJournalDataList(false, "Ошибка получения данных о типе объекта.");

            return GetJournalForItemKey(new ItemKey(itemType.IdItemType, relatedItem.IdBase));
        }

        /// <summary>
        /// Возвращает события, связанные с объектом с ключом <paramref name="itemKey"/> во всех журналах.
        /// </summary>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalDataList"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="itemKey"/> равен null.</exception>
        [ApiIrreversible]
        public ExecutionResultJournalDataList GetJournalForItemKey(ItemKey itemKey)
        {
            if (itemKey == null) throw new ArgumentNullException(nameof(itemKey));

            try
            {
                using (var db = new DB.DataContext())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress))
                {
                    var query = from item in db.ItemLink
                                join journalRow in DatabaseAccessor.CreateQueryJournalData(db) on item.LinkId equals journalRow.JournalData.ItemLinkId.Value
                                where item.ItemIdType == itemKey.IdType && item.ItemId == itemKey.IdItem && item.ItemKey == itemKey.Key
                                select journalRow;

                    var data = DatabaseAccessor.FetchQueryJournalData(query);

                    return new ExecutionResultJournalDataList(true, null, data);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager).FullName}.{nameof(JournalingManager.GetJournalForItemKey)}: {ex.ToString()}");
                return new ExecutionResultJournalDataList(false, $"Возникла ошибка во время получения событий. Смотрите информацию в системном текстовом журнале.");
            }
        }

        /// <summary>
        /// Возвращает событие с идентификатором <paramref name="idJournalData"/>. Все методы регистрации событий в результате содержат идентификатор созданной записи.
        /// </summary>
        /// <returns>
        /// Возвращает объект <see cref="ExecutionResultJournalData"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. 
        /// В случае успеха свойство <see cref="ExecutionResultJournalData.Result"/> содержит информацию о событии.
        /// В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.
        /// </returns>
        [ApiIrreversible]
        public ExecutionResultJournalData GetJournalData(int idJournalData)
        {
            try
            {
                using (var db = new DB.DataContext())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress))
                {
                    var query = DatabaseAccessor.CreateQueryJournalData(db).Where(x => x.JournalData.IdJournalData == idJournalData);
                    var data = DatabaseAccessor.FetchQueryJournalData(query).FirstOrDefault();

                    return new ExecutionResultJournalData(true, null, data);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager).FullName}.{nameof(JournalingManager.GetJournalForItem)}: {ex.ToString()}");
                return new ExecutionResultJournalData(false, $"Возникла ошибка во время получения события. Смотрите информацию в системном текстовом журнале.");
            }
        }

        /// <summary>
        /// Возвращает события на основании параметров поиска, указанных в <paramref name="dataFilterOptions"/>.
        /// </summary>
        /// <param name="dataFilterOptions">Параметры поиска записей.</param>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalDataList"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="dataFilterOptions"/> равен null.</exception>
        [ApiIrreversible]
        public ExecutionResultJournalDataList GetJournalData(DataFilterOptions dataFilterOptions)
        {
            if (dataFilterOptions == null) throw new ArgumentNullException(nameof(dataFilterOptions));

            int? idJournal = dataFilterOptions.IdJournal;
            if (dataFilterOptions.JournalComponentType != null)
            {
                var journalResult = GetJournalTyped(dataFilterOptions.JournalComponentType);
                if (!journalResult.IsSuccess) return new ExecutionResultJournalDataList(false, journalResult.Message);
            }

            try
            {
                using (var db = new DB.DataContext())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress))
                {
                    var query = DatabaseAccessor.CreateQueryJournalData(db);
                    if (idJournal.HasValue) query = query.Where(x => x.JournalName.IdJournal == idJournal);
                    if (!dataFilterOptions.EventCodes.IsNullOrEmpty()) query = query.In(dataFilterOptions.EventCodes.Distinct(), x => x.JournalData.EventCode);
                    if (dataFilterOptions.DateMin.HasValue) query = query.Where(x => x.JournalData.DateEvent >= dataFilterOptions.DateMin.Value);
                    if (dataFilterOptions.DateMax.HasValue) query = query.Where(x => x.JournalData.DateEvent <= dataFilterOptions.DateMax.Value);
                    if (dataFilterOptions.Limit.HasValue) query = query.Take(dataFilterOptions.Limit.Value);
                    var queryFinal = query.OrderByDescending(x => x.JournalData.DateEvent);
                    var data = DatabaseAccessor.FetchQueryJournalData(queryFinal);

                    return new ExecutionResultJournalDataList(true, null, data);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager).FullName}.{nameof(JournalingManager.GetJournalData)}: {ex.ToString()}");
                return new ExecutionResultJournalDataList(false, $"Возникла ошибка во время получения событий. Смотрите информацию в системном текстовом журнале.");
            }
        }

        #endregion

        #region Записать в журнал
        /// <summary>
        /// Регистрирует новое событие в журнале <paramref name="IdJournal"/>.
        /// </summary>
        /// <param name="IdJournal">См. <see cref="DB.JournalDAO.IdJournal"/>.</param>
        /// <param name="eventType">См. <see cref="DB.JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="DB.JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="DB.JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="DB.JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="DB.JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>
        /// Возвращает объект <see cref="ExecutionRegisterResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. 
        /// В случае успеха свойство <see cref="ExecutionRegisterResult.Result"/> содержит идентификатор записи журнала (см. также <see cref="GetJournalData(int)"/>).
        /// В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.
        /// </returns>
        [ApiIrreversible]
        public ExecutionRegisterResult RegisterEvent(int IdJournal, EventType eventType, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            return RegisterEventInternal(IdJournal, eventType, EventCodeDefault, eventInfo, eventInfoDetailed, eventTime, exception, null);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале <paramref name="IdJournal"/>.
        /// </summary>
        /// <param name="IdJournal">См. <see cref="DB.JournalDAO.IdJournal"/>.</param>
        /// <param name="eventType">См. <see cref="DB.JournalDAO.EventType"/>.</param>
        /// <param name="eventCode">См. <see cref="DB.JournalDAO.EventCode"/>.</param>
        /// <param name="eventInfo">См. <see cref="DB.JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="DB.JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="DB.JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="DB.JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>
        /// Возвращает объект <see cref="ExecutionRegisterResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. 
        /// В случае успеха свойство <see cref="ExecutionRegisterResult.Result"/> содержит идентификатор записи журнала (см. также <see cref="GetJournalData(int)"/>).
        /// В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.
        /// </returns>
        [ApiIrreversible]
        public ExecutionRegisterResult RegisterEvent(int IdJournal, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            return RegisterEventInternal(IdJournal, eventType, eventCode, eventInfo, eventInfoDetailed, eventTime, exception, null);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе типа <typeparamref name="TJournalTyped"/>.
        /// </summary>
        /// <param name="eventType">См. <see cref="DB.JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="DB.JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="DB.JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="DB.JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="DB.JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>
        /// Возвращает объект <see cref="ExecutionRegisterResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. 
        /// В случае успеха свойство <see cref="ExecutionRegisterResult.Result"/> содержит идентификатор записи журнала (см. также <see cref="GetJournalData(int)"/>).
        /// В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.
        /// </returns>
        [ApiIrreversible]
        public ExecutionRegisterResult RegisterEvent<TJournalTyped>(EventType eventType, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            return RegisterEvent(typeof(TJournalTyped), eventType, EventCodeDefault, eventInfo, eventInfoDetailed, eventTime, exception);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе типа <typeparamref name="TJournalTyped"/>.
        /// </summary>
        /// <param name="eventType">См. <see cref="DB.JournalDAO.EventType"/>.</param>
        /// <param name="eventCode">См. <see cref="DB.JournalDAO.EventCode"/>.</param>
        /// <param name="eventInfo">См. <see cref="DB.JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="DB.JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="DB.JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="DB.JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>
        /// Возвращает объект <see cref="ExecutionRegisterResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. 
        /// В случае успеха свойство <see cref="ExecutionRegisterResult.Result"/> содержит идентификатор записи журнала (см. также <see cref="GetJournalData(int)"/>).
        /// В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.
        /// </returns>
        [ApiIrreversible]
        public ExecutionRegisterResult RegisterEvent<TJournalTyped>(EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            return RegisterEvent(typeof(TJournalTyped), eventType, eventCode, eventInfo, eventInfoDetailed, eventTime, exception);
        }

        internal ExecutionRegisterResult RegisterEvent(Type typedType, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            typedType = ManagerExtensions.GetJournalType(typedType);

            try
            {
                var journalResult = GetJournalTyped(typedType);
                return !journalResult.IsSuccess ?
                    new ExecutionRegisterResult(false, journalResult.Message) :
                    RegisterEventInternal(journalResult.Result.IdJournal, eventType, eventCode, eventInfo, eventInfoDetailed, eventTime, exception, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager).FullName}.{nameof(JournalingManager.RegisterEvent)}: {ex.ToString()}");
                var fullName = TypeNameHelper.GetFullNameCleared(typedType);
                return new ExecutionRegisterResult(false, $"Возникла ошибка во время регистрации события в типизированный журнал '{fullName}'. Смотрите информацию в системном текстовом журнале.");
            }
        }

        /// <summary>
        /// Регистрирует новое событие, связанное с объектом <paramref name="relatedItem"/>, в журнале <paramref name="IdJournal"/>.
        /// </summary>
        /// <param name="IdJournal">См. <see cref="DB.JournalDAO.IdJournal"/>.</param>
        /// <param name="relatedItem">См. <see cref="DB.JournalDAO.IdJournal"/>.</param>
        /// <param name="eventType">См. <see cref="DB.JournalDAO.EventType"/>.</param>
        /// <param name="eventCode">См. <see cref="DB.JournalDAO.EventCode"/>.</param>
        /// <param name="eventInfo">См. <see cref="DB.JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="DB.JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="DB.JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="DB.JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>
        /// Возвращает объект <see cref="ExecutionRegisterResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. 
        /// В случае успеха свойство <see cref="ExecutionRegisterResult.Result"/> содержит идентификатор записи журнала (см. также <see cref="GetJournalData(int)"/>).
        /// В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.
        /// </returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="relatedItem"/> равен null.</exception>
        [ApiIrreversible]
        public ExecutionRegisterResult RegisterEventForItem(int IdJournal, ItemBase relatedItem, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            if (relatedItem == null) throw new ArgumentNullException(nameof(relatedItem));
            var itemType = ItemTypeFactory.GetItemType(relatedItem.GetType());
            if (itemType == null) return new ExecutionRegisterResult(false, "Ошибка получения данных о типе объекта.");

            return RegisterEventForItem(IdJournal, new ItemKey(itemType.IdItemType, relatedItem.IdBase), eventType, eventCode, eventInfo, eventInfoDetailed, eventTime, exception);
        }

        /// <summary>
        /// Регистрирует новое событие, связанное с объектом по ключу <paramref name="itemKey"/>, в журнале <paramref name="IdJournal"/>.
        /// </summary>
        /// <param name="IdJournal">См. <see cref="DB.JournalDAO.IdJournal"/>.</param>
        /// <param name="itemKey">См. <see cref="DB.JournalDAO.IdJournal"/>.</param>
        /// <param name="eventType">См. <see cref="DB.JournalDAO.EventType"/>.</param>
        /// <param name="eventCode">См. <see cref="DB.JournalDAO.EventCode"/>.</param>
        /// <param name="eventInfo">См. <see cref="DB.JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="DB.JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="DB.JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="DB.JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>
        /// Возвращает объект <see cref="ExecutionRegisterResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. 
        /// В случае успеха свойство <see cref="ExecutionRegisterResult.Result"/> содержит идентификатор записи журнала (см. также <see cref="GetJournalData(int)"/>).
        /// В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.
        /// </returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="itemKey"/> равен null.</exception>
        [ApiIrreversible]
        public ExecutionRegisterResult RegisterEventForItem(int IdJournal, ItemKey itemKey, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            if (itemKey == null) throw new ArgumentNullException(nameof(itemKey));
            var guid = AppCore.Get<ItemsManager>().RegisterItemLink(itemKey);
            return RegisterEventInternal(IdJournal, eventType, eventCode, eventInfo, eventInfoDetailed, eventTime, exception, guid);
        }

        /// <summary>
        /// Регистрирует новое событие, связанное с объектом <paramref name="relatedItem"/>, в журнале на основе типа <typeparamref name="TJournalTyped"/>.
        /// </summary>
        /// <param name="relatedItem">См. <see cref="DB.JournalDAO.IdJournal"/>.</param>
        /// <param name="eventType">См. <see cref="DB.JournalDAO.EventType"/>.</param>
        /// <param name="eventCode">См. <see cref="DB.JournalDAO.EventCode"/>.</param>
        /// <param name="eventInfo">См. <see cref="DB.JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="DB.JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="DB.JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="DB.JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>
        /// Возвращает объект <see cref="ExecutionRegisterResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. 
        /// В случае успеха свойство <see cref="ExecutionRegisterResult.Result"/> содержит идентификатор записи журнала (см. также <see cref="GetJournalData(int)"/>).
        /// В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.
        /// </returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="relatedItem"/> равен null.</exception>
        [ApiIrreversible]
        public ExecutionRegisterResult RegisterEventForItem<TJournalTyped>(ItemBase relatedItem, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            return RegisterEventForItem(typeof(TJournalTyped), relatedItem, eventType, eventCode, eventInfo, eventInfoDetailed, eventTime, exception);
        }

        /// <summary>
        /// Регистрирует новое событие, связанное с объектом с ключом <paramref name="itemKey"/>, в журнале на основе типа <typeparamref name="TJournalTyped"/>.
        /// </summary>
        /// <param name="itemKey">См. <see cref="DB.JournalDAO.IdJournal"/>.</param>
        /// <param name="eventType">См. <see cref="DB.JournalDAO.EventType"/>.</param>
        /// <param name="eventCode">См. <see cref="DB.JournalDAO.EventCode"/>.</param>
        /// <param name="eventInfo">См. <see cref="DB.JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="DB.JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="DB.JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="DB.JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>
        /// Возвращает объект <see cref="ExecutionRegisterResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. 
        /// В случае успеха свойство <see cref="ExecutionRegisterResult.Result"/> содержит идентификатор записи журнала (см. также <see cref="GetJournalData(int)"/>).
        /// В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.
        /// </returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="itemKey"/> равен null.</exception>
        [ApiIrreversible]
        public ExecutionRegisterResult RegisterEventForItem<TJournalTyped>(ItemKey itemKey, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            return RegisterEventForItem(typeof(TJournalTyped), itemKey, eventType, eventCode, eventInfo, eventInfoDetailed, eventTime, exception);
        }

        internal ExecutionRegisterResult RegisterEventForItem(Type typedType, ItemBase relatedItem, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            if (relatedItem == null) throw new ArgumentNullException(nameof(relatedItem));
            var itemType = ItemTypeFactory.GetItemType(relatedItem.GetType());
            if (itemType == null) return new ExecutionRegisterResult(false, "Ошибка получения данных о типе объекта.");

            return RegisterEventForItem(typedType, new ItemKey(itemType.IdItemType, relatedItem.IdBase), eventType, eventCode, eventInfo, eventInfoDetailed, eventTime, exception);
        }

        internal ExecutionRegisterResult RegisterEventForItem(Type typedType, ItemKey itemKey, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            typedType = ManagerExtensions.GetJournalType(typedType);

            if (itemKey == null) throw new ArgumentNullException(nameof(itemKey));
            var guid = AppCore.Get<ItemsManager>().RegisterItemLink(itemKey);

            try
            {
                var journalResult = GetJournalTyped(typedType);
                return !journalResult.IsSuccess ?
                    new ExecutionRegisterResult(false, journalResult.Message) :
                    RegisterEventInternal(journalResult.Result.IdJournal, eventType, eventCode, eventInfo, eventInfoDetailed, eventTime, exception, guid);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager).FullName}.{nameof(JournalingManager.RegisterEventForItem)}: {ex.ToString()}");
                var fullName = TypeNameHelper.GetFullNameCleared(typedType);
                return new ExecutionRegisterResult(false, $"Возникла ошибка во время регистрации события в типизированный журнал '{fullName}'. Смотрите информацию в системном текстовом журнале.");
            }
        }

        private ExecutionRegisterResult RegisterEventInternal(int IdJournal, EventType eventType, int eventCode, string eventInfo, string eventInfoDetailed, DateTime? eventTime, Exception exception, Guid? itemLinkId)
        {
            try
            {
                if (IdJournal <= 0) throw new ArgumentOutOfRangeException(nameof(IdJournal));
                if (string.IsNullOrEmpty(eventInfo)) throw new ArgumentNullException(nameof(eventInfo));

                var exceptionDetailed = exception != null ? (exception.GetMessageExtended() + "\r\n" + exception.ToString()) : null;
                if (!string.IsNullOrEmpty(exceptionDetailed))
                {
                    var pos = exceptionDetailed.IndexOf("System.Web.Mvc.ActionMethodDispatcher.Execute", StringComparison.InvariantCultureIgnoreCase);
                    if (pos >= 0) exceptionDetailed = exceptionDetailed.Substring(0, pos);
                }

                var idUser = AppCore.GetUserContextManager().GetCurrentUserContext()?.IdUser;
                if (idUser == 0) idUser = null;

                var data = new DB.JournalDAO()
                {
                    IdJournal = IdJournal,
                    EventType = eventType,
                    EventInfo = eventInfo?.Truncate(0, 300),
                    EventInfoDetailed = eventInfoDetailed,
                    ExceptionDetailed = exceptionDetailed,
                    EventCode = eventCode,
                    DateEvent = eventTime ?? DateTime.Now,
                    IdUser = idUser,
                    ItemLinkId = itemLinkId
                };

                using (var db = new DB.DataContext())
                {
                    DB.JournalNameDAO journalForCritical = null;

                    using (var scope = db.CreateScope(TransactionScopeOption.RequiresNew))
                    {
                        db.Journal.Add(data);
                        db.SaveChanges();

                        if (eventType == EventType.CriticalError)
                        {
                            journalForCritical = db.JournalName.Where(x => x.IdJournal == IdJournal).FirstOrDefault();
                        }
                        scope.Complete();
                    }

                    if (eventType == EventType.CriticalError)
                    {
                        AppCore.Get<Modules.Subscriptions.SubscriptionsManager>().
                            AsParametrized(new CriticalJournalEventSubscription.Parameters()
                            {
                                JournalInfo = journalForCritical,
                                EventInfo = data
                            }).
                            ExecuteSubscription<CriticalJournalEventSubscription>();
                    }

                }

                return new ExecutionRegisterResult(true, null, data.IdJournalData);
            }
            catch (HandledException ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager).FullName}.{nameof(JournalingManager.RegisterEvent)}: {ex.InnerException?.ToString()}");
                return new ExecutionRegisterResult(false, $"Возникла ошибка во время регистрации события в журнал №{IdJournal}. {ex.Message} Смотрите информацию в системном текстовом журнале.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager).FullName}.{nameof(JournalingManager.RegisterEvent)}: {ex.ToString()}");
                return new ExecutionRegisterResult(false, $"Возникла ошибка во время регистрации события в журнал №{IdJournal}. Смотрите информацию в системном текстовом журнале.");
            }
        }
        #endregion

        #region Свойства
        /// <summary>
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public DB.JournalingManagerDatabaseAccessor DatabaseAccessor { get; private set; }
        #endregion
    }
}
