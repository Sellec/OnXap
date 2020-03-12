﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnXap.Modules.Lexicon
{
    using Core;
    using Db;
    using Journaling;

    /// <summary>
    /// Менеджер для работы со словарными формами.
    /// </summary>
    public class LexiconManager : CoreComponentBase, IComponentSingleton
    {
        /// <summary>
        /// Структура для запроса числительной и падежной формы слова.
        /// </summary>
        public class Request
        {
            /// <summary>
            /// Слово в именительном падеже единственном числе (см. <see cref="eNumeralType.SingleType"/>).
            /// </summary>
            public string Word { get; set; }

            /// <summary>
            /// Результат выполнения запроса.
            /// </summary>
            public WordCase Result { get; internal set; }
        }

        private static Lazy<OnlineMorpher> _morpher = new Lazy<OnlineMorpher>();
        private Dictionary<string, WordCase> _cache = null;

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStarting()
        {
            AppCore.Get<JournalingManager>().RegisterJournalTyped<LexiconManager>("Журнал лексического менеджера");
            UpdateCache();
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion

        //private static string GetWordCase(string word, int count, Func<WordCase, string> wordCaseCallback, string wordDefault = null)
        //{
        //    var request = Get(word, count);
        //    if (request != null && request.Result != null)
        //    {
        //        var wordCase = wordCaseCallback(request.Result);
        //        if (!string.IsNullOrEmpty(wordCase)) return wordCase;
        //    }
        //    return wordDefault;
        //}

        ///// <summary>
        ///// Возвращает именительный падеж слова <paramref name="word"/> в количественном варианте <paramref name="count"/> (см. <see cref="Request.Count"/>).
        ///// </summary>
        ///// <param name="word">См. <see cref="Request.Word"/>.</param>
        ///// <param name="count">См. <see cref="Request.Count"/>.</param>
        ///// <param name="wordDefault">Возвращается в качестве результата выполнения функции в случае, когда подходящая форма слова не была найдена.</param>
        ///// <returns></returns>
        //public static string GetNominative(string word, int count, string wordDefault = null)
        //{
        //    return GetWordCase(word, count, wordCases => wordCases.Nominative, wordDefault);
        //}

        ///// <summary>
        ///// Возвращает родительный падеж слова <paramref name="word"/> в количественном варианте <paramref name="count"/> (см. <see cref="Request.Count"/>).
        ///// </summary>
        ///// <param name="word">См. <see cref="Request.Word"/>.</param>
        ///// <param name="count">См. <see cref="Request.Count"/>.</param>
        ///// <param name="wordDefault">Возвращается в качестве результата выполнения функции в случае, когда подходящая форма слова не была найдена.</param>
        ///// <returns></returns>
        //public static string GetGenitive(string word, int count, string wordDefault = null)
        //{
        //    return GetWordCase(word, count, wordCases => wordCases.Genitive, wordDefault);
        //}

        ///// <summary>
        ///// Возвращает дательный падеж слова <paramref name="word"/> в количественном варианте <paramref name="count"/> (см. <see cref="Request.Count"/>).
        ///// </summary>
        ///// <param name="word">См. <see cref="Request.Word"/>.</param>
        ///// <param name="count">См. <see cref="Request.Count"/>.</param>
        ///// <param name="wordDefault">Возвращается в качестве результата выполнения функции в случае, когда подходящая форма слова не была найдена.</param>
        ///// <returns></returns>
        //public static string GetDative(string word, int count, string wordDefault = null)
        //{
        //    return GetWordCase(word, count, wordCases => wordCases.Dative, wordDefault);
        //}

        ///// <summary>
        ///// Возвращает винительный падеж слова <paramref name="word"/> в количественном варианте <paramref name="count"/> (см. <see cref="Request.Count"/>).
        ///// </summary>
        ///// <param name="word">См. <see cref="Request.Word"/>.</param>
        ///// <param name="count">См. <see cref="Request.Count"/>.</param>
        ///// <param name="wordDefault">Возвращается в качестве результата выполнения функции в случае, когда подходящая форма слова не была найдена.</param>
        ///// <returns></returns>
        //public static string GetAccusative(string word, int count, string wordDefault = null)
        //{
        //    return GetWordCase(word, count, wordCases => wordCases.Accusative, wordDefault);
        //}

        ///// <summary>
        ///// Возвращает творительный падеж слова <paramref name="word"/> в количественном варианте <paramref name="count"/> (см. <see cref="Request.Count"/>).
        ///// </summary>
        ///// <param name="word">См. <see cref="Request.Word"/>.</param>
        ///// <param name="count">См. <see cref="Request.Count"/>.</param>
        ///// <param name="wordDefault">Возвращается в качестве результата выполнения функции в случае, когда подходящая форма слова не была найдена.</param>
        ///// <returns></returns>
        //public static string GetInstrumental(string word, int count, string wordDefault = null)
        //{
        //    return GetWordCase(word, count, wordCases => wordCases.Instrumental, wordDefault);
        //}

        ///// <summary>
        ///// Возвращает предложный падеж слова <paramref name="word"/> в количественном варианте <paramref name="count"/> (см. <see cref="Request.Count"/>).
        ///// </summary>
        ///// <param name="word">См. <see cref="Request.Word"/>.</param>
        ///// <param name="count">См. <see cref="Request.Count"/>.</param>
        ///// <param name="wordDefault">Возвращается в качестве результата выполнения функции в случае, когда подходящая форма слова не была найдена.</param>
        ///// <returns></returns>
        //public static string GetPrepositional(string word, int count, string wordDefault = null)
        //{
        //    return GetWordCase(word, count, wordCases => wordCases.Prepositional, wordDefault);
        //}

        /// <summary>
        /// Обрабатывает один запрос на получение формы слова <paramref name="word"/>.
        /// </summary>
        /// <param name="word">См. <see cref="Request.Word"/>.</param>
        /// <returns></returns>
        public Request Get(string word)
        {
            return Get(new List<Request>() { new Request() { Word = word } }).FirstOrDefault();
        }

        /// <summary>
        /// Обрабатывает список запросов на получение форм слов.
        /// </summary>
        /// <returns>Возвращает переданный список запросов, для успешно обработанных запросов заполняется свойство <see cref="Request.Result"/>.</returns>
        public IEnumerable<Request> Get(List<Request> requestList)
        {
            try
            {
                // todo setError(null);

                using (var db = new DataContext())
                {
                    var results = new List<WordCase>();

                    var cache = _cache;

                    foreach (var request in requestList)
                    {
                        if (cache != null)
                        {
                            if (cache.TryGetValue(request.Word, out var value))
                            {
                                results.Add(value);
                                continue;
                            }
                        }

                        var result = db.WordCase.Where(x => x.NominativeSingle == request.Word).FirstOrDefault();
                        if (result != null)
                        {
                            if (cache != null) cache[result.NominativeSingle] = result;
                            results.Add(result);
                            continue;
                        }
                    }

                    foreach (var res in results)
                    {
                        var requests = requestList.Where(x => x.Word == res.NominativeSingle);
                        requests.ForEach(x => x.Result = res);
                    }

                    foreach (var request in requestList)
                    {
                        if (request.Result == null || request.Result.IsNewSingle || request.Result.IsNewTwo || request.Result.IsNewMultiple)
                        {
                            var numeralTypesList = new List<eNumeralType>();
                            if (request.Result == null || request.Result.IsNewSingle) numeralTypesList.Add(eNumeralType.SingleType);
                            if (request.Result == null || request.Result.IsNewTwo) numeralTypesList.Add(eNumeralType.TwoThreeFour);
                            if (request.Result == null || request.Result.IsNewMultiple) numeralTypesList.Add(eNumeralType.Multiple);

                            var list = numeralTypesList.ToDictionary(x => x, x => _morpher.Value.GetNumeralResult(request.Word, x));

                            if (request.Result == null)
                                request.Result = new WordCase()
                                {
                                    NominativeSingle = request.Word,
                                    IsNewSingle = true,
                                    IsNewTwo = true,
                                    IsNewMultiple = true
                                };

                            if (list.ContainsKey(eNumeralType.SingleType) && list[eNumeralType.SingleType] != null)
                            {
                                request.Result.GenitiveSingle = list[eNumeralType.SingleType].Р ?? "";
                                request.Result.DativeSingle = list[eNumeralType.SingleType].Д ?? "";
                                request.Result.AccusativeSingle = list[eNumeralType.SingleType].В ?? "";
                                request.Result.InstrumentalSingle = list[eNumeralType.SingleType].Т ?? "";
                                request.Result.PrepositionalSingle = list[eNumeralType.SingleType].П ?? "";
                                request.Result.IsNewSingle = false;

                                db.WordCase.
                                    UpsertRange(request.Result).
                                    On(x => x.NominativeSingle).
                                    WhenMatched((xDb, xIns) => new WordCase()
                                    {
                                        GenitiveSingle = xIns.GenitiveSingle,
                                        DativeSingle = xIns.DativeSingle,
                                        AccusativeSingle = xIns.AccusativeSingle,
                                        InstrumentalSingle = xIns.InstrumentalSingle,
                                        PrepositionalSingle = xIns.PrepositionalSingle,
                                        IsNewSingle = xIns.IsNewSingle
                                    }).
                                    Run();
                            }

                            if (list.ContainsKey(eNumeralType.TwoThreeFour) && list[eNumeralType.TwoThreeFour] != null)
                            {
                                request.Result.NominativeTwo = list[eNumeralType.TwoThreeFour].И ?? "";
                                request.Result.GenitiveTwo = list[eNumeralType.TwoThreeFour].Р ?? "";
                                request.Result.DativeTwo = list[eNumeralType.TwoThreeFour].Д ?? "";
                                request.Result.AccusativeTwo = list[eNumeralType.TwoThreeFour].В ?? "";
                                request.Result.InstrumentalTwo = list[eNumeralType.TwoThreeFour].Т ?? "";
                                request.Result.PrepositionalTwo = list[eNumeralType.TwoThreeFour].П ?? "";
                                request.Result.IsNewTwo = false;

                                db.WordCase.
                                    UpsertRange(request.Result).
                                    On(x => x.NominativeSingle).
                                    WhenMatched((xDb, xIns) => new WordCase()
                                    {
                                        NominativeTwo = xIns.NominativeTwo,
                                        GenitiveTwo = xIns.GenitiveTwo,
                                        DativeTwo = xIns.DativeTwo,
                                        AccusativeTwo = xIns.AccusativeTwo,
                                        InstrumentalTwo = xIns.InstrumentalTwo,
                                        PrepositionalTwo = xIns.PrepositionalTwo,
                                        IsNewTwo = xIns.IsNewTwo
                                    }).
                                    Run();
                            }

                            if (list.ContainsKey(eNumeralType.Multiple) && list[eNumeralType.Multiple] != null)
                            {
                                request.Result.NominativeMultiple = list[eNumeralType.Multiple].И ?? "";
                                request.Result.GenitiveMultiple = list[eNumeralType.Multiple].Р ?? "";
                                request.Result.DativeMultiple = list[eNumeralType.Multiple].Д ?? "";
                                request.Result.AccusativeMultiple = list[eNumeralType.Multiple].В ?? "";
                                request.Result.InstrumentalMultiple = list[eNumeralType.Multiple].Т ?? "";
                                request.Result.PrepositionalMultiple = list[eNumeralType.Multiple].П ?? "";
                                request.Result.IsNewMultiple = false;

                                db.WordCase.
                                    UpsertRange(request.Result).
                                    On(x => x.NominativeSingle).
                                    WhenMatched((xDb, xIns) => new WordCase()
                                    {
                                        NominativeMultiple = xIns.NominativeMultiple,
                                        GenitiveMultiple = xIns.GenitiveMultiple,
                                        DativeMultiple = xIns.DativeMultiple,
                                        AccusativeMultiple = xIns.AccusativeMultiple,
                                        InstrumentalMultiple = xIns.InstrumentalMultiple,
                                        PrepositionalMultiple = xIns.PrepositionalMultiple,
                                        IsNewMultiple = xIns.IsNewMultiple
                                    }).
                                    Run();
                            }
                        }
                    }

                    return requestList;
                }
            }
            catch (Exception ex)
            {
                // todo setError(ex.Message);
                this.RegisterEvent(
                    EventType.Error,
                    "Get: ошибка получения данных.",
                    null,
                    null,
                    ex
                );
            }

            return null;
        }

        private void UpdateCache()
        {
            try
            {
                int cacheLimitAllowed = 50000;
                var cache = new Dictionary<string, WordCase>();
                using (var db = new Db.DataContext())
                {
                    for (int i = 0; i <= cacheLimitAllowed; i += 5000)
                    {
                        var query = db.WordCase.Where(x => !x.IsNewSingle).OrderBy(x => x.NominativeSingle).Skip(i).Take(5000);
                        var rows = query.ToList();
                        rows.ForEach(x => cache[x.NominativeSingle] = x);
                        if (rows.Count < 5000) break;
                    }
                }

                _cache = cache;
            }
            catch (Exception ex)
            {
                this.RegisterEvent(EventType.Error, "Ошибка заполнения кеша", null, ex);
            }
        }

        /// <summary>
        /// Обрабатывает новые добавленные слова, получая для них словарные формы.
        /// </summary>
        public void PrepareNewWords()
        {
            try
            {
                using (var db = new DataContext())
                {
                    var queryWords = db.WordCase.Where(x => x.IsNewSingle || x.IsNewTwo || x.IsNewMultiple).ToList();
                    foreach (var word in queryWords)
                    {
                        try
                        {
                            var numeralTypesList = new List<eNumeralType>();
                            if (word.IsNewSingle) numeralTypesList.Add(eNumeralType.SingleType);
                            if (word.IsNewTwo) numeralTypesList.Add(eNumeralType.TwoThreeFour);
                            if (word.IsNewMultiple) numeralTypesList.Add(eNumeralType.Multiple);

                            var list = numeralTypesList.ToDictionary(x => x, x => _morpher.Value.GetNumeralResult(word.NominativeSingle, x));

                            if (list.ContainsKey(eNumeralType.SingleType) && list[eNumeralType.SingleType] != null)
                            {
                                word.GenitiveSingle = list[eNumeralType.SingleType].Р;
                                word.DativeSingle = list[eNumeralType.SingleType].Д;
                                word.AccusativeSingle = list[eNumeralType.SingleType].В;
                                word.InstrumentalSingle = list[eNumeralType.SingleType].Т;
                                word.PrepositionalSingle = list[eNumeralType.SingleType].П;
                                word.IsNewSingle = false;

                                db.WordCase.
                                    UpsertRange(word).
                                    On(x => x.NominativeSingle).
                                    WhenMatched((xDb, xIns) => new WordCase()
                                    {
                                        GenitiveSingle = xIns.GenitiveSingle,
                                        DativeSingle = xIns.DativeSingle,
                                        AccusativeSingle = xIns.AccusativeSingle,
                                        InstrumentalSingle = xIns.InstrumentalSingle,
                                        PrepositionalSingle = xIns.PrepositionalSingle,
                                        IsNewSingle = xIns.IsNewSingle
                                    }).
                                    Run();
                            }

                            if (list.ContainsKey(eNumeralType.TwoThreeFour) && list[eNumeralType.TwoThreeFour] != null)
                            {
                                word.NominativeTwo = list[eNumeralType.TwoThreeFour].И;
                                word.GenitiveTwo = list[eNumeralType.TwoThreeFour].Р;
                                word.DativeTwo = list[eNumeralType.TwoThreeFour].Д;
                                word.AccusativeTwo = list[eNumeralType.TwoThreeFour].В;
                                word.InstrumentalTwo = list[eNumeralType.TwoThreeFour].Т;
                                word.PrepositionalTwo = list[eNumeralType.TwoThreeFour].П;
                                word.IsNewTwo = false;

                                db.WordCase.
                                    UpsertRange(word).
                                    On(x => x.NominativeSingle).
                                    WhenMatched((xDb, xIns) => new WordCase()
                                    {
                                        NominativeTwo = xIns.NominativeTwo,
                                        GenitiveTwo = xIns.GenitiveTwo,
                                        DativeTwo = xIns.DativeTwo,
                                        AccusativeTwo = xIns.AccusativeTwo,
                                        InstrumentalTwo = xIns.InstrumentalTwo,
                                        PrepositionalTwo = xIns.PrepositionalTwo,
                                        IsNewTwo = xIns.IsNewTwo
                                    }).
                                    Run();
                            }

                            if (list.ContainsKey(eNumeralType.Multiple) && list[eNumeralType.Multiple] != null)
                            {
                                word.NominativeMultiple = list[eNumeralType.Multiple].И;
                                word.GenitiveMultiple = list[eNumeralType.Multiple].Р;
                                word.DativeMultiple = list[eNumeralType.Multiple].Д;
                                word.AccusativeMultiple = list[eNumeralType.Multiple].В;
                                word.InstrumentalMultiple = list[eNumeralType.Multiple].Т;
                                word.PrepositionalMultiple = list[eNumeralType.Multiple].П;
                                word.IsNewMultiple = false;

                                db.WordCase.
                                    UpsertRange(word).
                                    On(x => x.NominativeSingle).
                                    WhenMatched((xDb, xIns) => new WordCase()
                                    {
                                        NominativeMultiple = xIns.NominativeMultiple,
                                        GenitiveMultiple = xIns.GenitiveMultiple,
                                        DativeMultiple = xIns.DativeMultiple,
                                        AccusativeMultiple = xIns.AccusativeMultiple,
                                        InstrumentalMultiple = xIns.InstrumentalMultiple,
                                        PrepositionalMultiple = xIns.PrepositionalMultiple,
                                        IsNewMultiple = xIns.IsNewMultiple
                                    }).
                                    Run();
                            }
                        }
                        catch (Exception ex)
                        {
                            this.RegisterEvent(
                                        EventType.Error,
                                        $"Get: ошибка получения данных для слова '{word.NominativeSingle}'.",
                                        null,
                                        null,
                                        ex
                            );
                        }
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // todo setError(ex.Message);
                this.RegisterEvent(
                            EventType.Error,
                            "Get: ошибка получения данных.",
                            null,
                            null,
                            ex
                        );
            }

            UpdateCache();
        }
    }
}


