using System;
using System.Collections.Concurrent;
using System.Linq;

namespace OnXap
{
    static class DeprecatedApplicationHolder
    {
        private static object SyncRoot = new object();
        private static ConcurrentDictionary<Type, object> _applicationCoreInstances = new ConcurrentDictionary<Type, object>();

        public static OnXApplication Get()
        {
            lock (SyncRoot)
            {
                return _applicationCoreInstances.Where(x => x.Key == typeof(OnXApplication)).Select(x => (OnXApplication)x.Value).FirstOrDefault();
            }
        }

        public static void Set(OnXApplication applicationCore)
        {
            lock (SyncRoot)
            {
                _applicationCoreInstances.AddOrUpdate(
                    typeof(OnXApplication),
                    key => applicationCore,
                    (key, old) =>
                    {
                        if (old != applicationCore) Debug.WriteLine($"Установлен новый экземпляр приложения '{applicationCore.GetType().FullName}' на базе '{typeof(OnXApplication).FullName}'. Возможны проблемы с определением активного модуля при создании экземпляров ItemBase. Для корректной работы убедитесь, что предыдущее зарегистрированное ядро такого типа было остановлено.");
                        return applicationCore;
                    }
                );
            }
        }

        public static void Remove(OnXApplication applicationCore)
        {
            lock (SyncRoot)
            {
                if (_applicationCoreInstances.TryRemove(typeof(OnXApplication), out var instance) && instance != applicationCore)
                {
                    _applicationCoreInstances.TryAdd(typeof(OnXApplication), instance);
                }
            }
        }


    }
}
