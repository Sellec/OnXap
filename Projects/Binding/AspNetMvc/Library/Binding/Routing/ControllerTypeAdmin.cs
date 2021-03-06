﻿using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace OnXap.Binding.Routing
{
    using Core.Modules;

    /// <summary>
    /// См. <see cref="ControllerType"/>. Обозначает контроллер для панели управления.
    /// </summary>
    public class ControllerTypeAdmin : ControllerType
    {
        /// <summary>
        /// Обозначение типа контроллера.
        /// </summary>
        public const int TypeID = 2;

        /// <summary>
        /// Создает новый экземпляр объекта.
        /// </summary>
        public ControllerTypeAdmin() : base(2, "Контроллер панели управления")
        {
        }

        /// <summary>
        /// См. <see cref="ControllerType.IsThisRequestIsThisControllerType(HttpRequestBase, string)"/>.
        /// </summary>
        public override bool IsThisRequestIsThisControllerType(HttpRequestBase request, string relativeURL)
        {
            return request.RequestContext.RouteData.DataTokens.TryGetValue("area", out object value) && value is string && value.ToString() == AreaConstants.AdminPanel;
        }

        /// <summary>
        /// См. <see cref="ControllerType.ErrorCannotFindControllerTypeSpecified(IModuleCore, RouteValueDictionary)"/>.
        /// </summary>
        public override string ErrorCannotFindControllerTypeSpecified(IModuleCore module, RouteValueDictionary routeValues)
        {
            return string.Format("Модуль '{0}' не предоставляет доступ в панель управления.", module.Caption);
        }

        /// <summary>
        /// См. <see cref="CheckPermissions(IModuleCore, RouteValueDictionary)"/>.
        /// </summary>
        public override bool CheckPermissions(IModuleCore module, RouteValueDictionary routeValues)
        {
            //Проверка доступа к саму панель управления. Нет прав на модуле "Admin".
            var moduleAdmin = AppCore.GetModulesManager().GetModule<Modules.Admin.ModuleAdmin>();
            if (moduleAdmin != null)
            {
                if (moduleAdmin.CheckPermission(ModulesConstants.PermissionManage) != CheckPermissionVariant.Allowed)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// См. <see cref="CreateRelativeUrl(string, string, string[])"/>.
        /// </summary>
        public override string CreateRelativeUrl(string moduleName, string actionName, string[] parameters = null)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(moduleName)) parts.Add(moduleName);
            if (!string.IsNullOrEmpty(actionName)) parts.Add(actionName);
            parts.AddRange(parameters.Where(x => !string.IsNullOrEmpty(x)));

            if (parts.Count == 0) return "/admin";
            else return $"/admin/mnadmin/{string.Join("/", parts)}";
        }
    }
}