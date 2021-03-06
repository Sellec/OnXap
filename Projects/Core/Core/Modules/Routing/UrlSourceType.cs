﻿using System.Collections.Generic;

namespace OnXap.Modules.Routing
{
    using Core.Items;
    using Core.Modules;

    /// <summary>
    /// Перечисляет виды источников url-адресов объектов (см. <see cref="IItemRouted.Url"/>).
    /// </summary>
    public enum UrlSourceType
    {
        /// <summary>
        /// Используется в случае отсутствия url-адреса (когда <see cref="IItemRouted.Url"/> равен null или empty).
        /// </summary>
        None,

        /// <summary>
        /// Адрес из основной таблицы маршрутизации (зарегистрированный через <see cref="UrlManager"/> с ключом <see cref="RoutingConstants.MAINKEY"/>).
        /// </summary>
        /// <seealso cref="UrlManager.Register{TModuleType}(ModuleCore{TModuleType}, IEnumerable{RegisterItem})"/>
        /// <seealso cref="UrlManager.Register{TModuleType}(ModuleCore{TModuleType}, int, int, string, IEnumerable{ActionArgument}, string, string)"/>
        Routing,

        /// <summary>
        /// Адрес, сгенерированный модулем.
        /// </summary>
        /// <seealso cref="ModuleCore.GenerateLink(ItemBase)"/>
        /// <seealso cref="ModuleCore.GenerateLinks(IEnumerable{ItemBase})"/>
        Module,
    }
}
