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
