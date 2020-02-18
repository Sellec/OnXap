using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OnUtils.Architecture.AppCore;

namespace OnXap.Core.Modules
{
    public class ModuleControllerTypesManager : CoreComponentBase, IModuleRegisteredHandler
    {
        private static MethodInfo _methodInfo = typeof(ModuleControllerTypesManager).GetMethod(nameof(OnModuleInitialized), BindingFlags.Instance | BindingFlags.NonPublic);
        private ConcurrentDictionary<Type, Dictionary<int, Type>> _moduleControllerTypesList = new ConcurrentDictionary<Type, Dictionary<int, Type>>();

        #region CoreComponentBase
        protected override void OnStarting()
        {
        }

        protected override void OnStop()
        {
        }
        #endregion

        #region IModuleRegisteredHandler
        void IModuleRegisteredHandler.OnModuleInitialized<TModule>(TModule module)
        {
            if (AppCore.AppDebugLevel >= DebugLevel.Common)
                Debug.WriteLine($"{nameof(ModuleControllerTypesManager)}.{nameof(IModuleRegisteredHandler)}.{nameof(IModuleRegisteredHandler.OnModuleInitialized)}: перехват инициализации модуля '{typeof(TModule)}' ('{module.GetType()}').");

            _methodInfo.MakeGenericMethod(typeof(TModule)).Invoke(this, new object[] { module });
        }

        private void OnModuleInitialized<TModuleType>(TModuleType module) where TModuleType : ModuleCore<TModuleType>
        {
            var controllerTypes = AppCore.GetBindedTypes<IModuleController<TModuleType>>();

            if (controllerTypes != null)
            {
                if (AppCore.AppDebugLevel >= DebugLevel.Detailed)
                    Debug.WriteLine($"{nameof(ModuleControllerTypesManager)}.{nameof(OnModuleInitialized)}: найдены следующие типы контроллеров - '{string.Join("', '", controllerTypes)}'.");

                var controllerTypesSplitIntoTypes = controllerTypes.
                    Select(x => new { Type = x, Attribute = x.GetCustomAttribute<ModuleControllerAttribute>() }).
                    Where(x => x.Attribute != null).
                    GroupBy(x => x.Attribute.ControllerTypeID, x => x.Type).
                    Select(x => new { ControllerTypeID = x.Key, ControllerType = x.Last() }).
                    ToList();

                _moduleControllerTypesList[module.QueryType] = controllerTypesSplitIntoTypes.ToDictionary(x => x.ControllerTypeID, x => x.ControllerType);
            }
            else
            {
                if (AppCore.AppDebugLevel >= DebugLevel.Detailed)
                    Debug.WriteLine($"{nameof(ModuleControllerTypesManager)}.{nameof(OnModuleInitialized)}: не найдено ни одного типа контроллеров.");

                _moduleControllerTypesList[module.QueryType] = new Dictionary<int, Type>();
            }
        }
        #endregion

        public Dictionary<int, Type> GetModuleControllerTypes<TModuleType>() where TModuleType : ModuleCore<TModuleType>
        {
            return GetModuleControllerTypes(typeof(TModuleType));
        }

        public Dictionary<int, Type> GetModuleControllerTypes(Type moduleType)
        {
            return _moduleControllerTypesList.TryGetValue(moduleType, out var value) ? value : new Dictionary<int, Type>();
        }
    }
}
