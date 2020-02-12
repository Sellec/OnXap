using System;
using System.Collections.Concurrent;
using System.Linq;

namespace OnXap
{
    using Core.Modules;

    /// <summary>
    /// Будет удалено в будущих версиях.
    /// </summary>
    public static class DeprecatedSingletonInstances
    {
        private static ConcurrentDictionary<Type, object> _modulesManagers = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// </summary>
        [Obsolete("Будет удалено в будущих версиях.")]
        public static ModulesManager Get()
        {
            return _modulesManagers.Where(x => x.Key == typeof(OnXApplication)).Select(x => x.Value as ModulesManager).FirstOrDefault();
        }

        /// <summary>
        /// </summary>
        [Obsolete("Будет удалено в будущих версиях.")]
        public static void Set(ModulesManager manager)
        {
            _modulesManagers[typeof(OnXApplication)] = manager;
        }

    }
}
