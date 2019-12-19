using System;
using System.Collections.Concurrent;

namespace OnXap.Utils
{
    /// <summary>
    /// Предоставляет возможность получать уникальные идентификаторы. 
    /// Каждое обращение к <see cref="GetUnique"/> гарантированно возвращает идентификатор, который ранее не использовался.
    /// Поддерживается многопоточность.
    /// </summary>
    public static class GuidGenerator
    {
        private static readonly object _syncRoot = new object();
        private static ConcurrentQueue<Guid> _queue = new ConcurrentQueue<Guid>();
        private static byte[] _guidPrefix = null;

        static GuidGenerator()
        {
            _guidPrefix = new byte[8];
            Array.Copy(BitConverter.GetBytes(long.MaxValue), _guidPrefix, 8);
        }

        private static Guid ToGuid(long value)
        {
            byte[] guidData = new byte[16];
            Array.Copy(_guidPrefix, 0, guidData, 0, 8);
            Array.Copy(BitConverter.GetBytes(value), 0, guidData, 8, 8);
            return new Guid(guidData);
        }

        /// <summary>
        /// Каждое обращение гарантированно возвращает идентификатор, который ранее не использовался.
        /// Поддерживается многопоточность.
        /// </summary>
        public static Guid GetUnique()
        {
            //todo рассмотреть следующий алгоритм:
            /*
            private static object tickCounterSyncRoot = new object();
            private static long tickStart = 0;
            private static volatile int tickCounter = 0;

            static void Main(string[] args)
            {
                tickStart = DateTime.Now.Ticks;

                var t = DateTime.Now;
                var list = new System.Collections.Concurrent.ConcurrentQueue<Guid>();
                var list2 = new System.Collections.Concurrent.ConcurrentQueue<long>();
                var tasks = new List<Task>();

                for (int i = 0; i < 50; i++)
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        for (int k = 0; k < 200000; k++)
                        {
                            var c = tickStart;
                            lock (tickCounterSyncRoot)
                            {
                                c += tickCounter;
                                tickCounter++;
                            }
                            //var guid = c.ToString().GenerateGuid();list.Enqueue(guid);
                            list2.Enqueue(c);
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
                var t2 = DateTime.Now - t;
                var dd = list.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => new { x.Key, count = x.Count() }).ToList();
                var dd2 = list2.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => new { x.Key, count = x.Count() }).ToList();

                Console.ReadKey();
            }
            */

            for (int a = 0; a < 100; a++)
            {
                if (_queue.IsEmpty)
                {
                    lock (_syncRoot)
                    {
                        if (_queue.IsEmpty)
                        {
                            var ticks = DateTime.Now.Ticks;
                            var guids = new Guid[5000];
                            for (int j = 0; j < guids.Length; j++)
                            {
                                guids[j] = ToGuid(ticks + j);
                            }
                            if ((DateTime.Now.Ticks - ticks) < 10000) System.Threading.Thread.Sleep(1);
                            _queue = new ConcurrentQueue<Guid>(guids);
                        }
                    }
                }
                if (_queue.TryDequeue(out var guid))
                {
                    return guid;
                }
            }
            throw new InvalidOperationException("Ошибка генерации идентификатора.");
        }
    }
}
