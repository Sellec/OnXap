﻿using System.Web;
using System.Web.Routing;

namespace OnXap.Binding.Routing
{
    using Core;
    using Core.Modules;

    /// <summary>
    /// Описывает тип контроллера и предоставляет способ выбрать нужный тип контроллера во время обработки входящего запроса.
    /// </summary>
    public abstract class ControllerType : CoreComponentBase
    {
        /// <summary>
        /// </summary>
        internal ControllerType(int controllerTypeID, string controllerTypeName)
        {
            ControllerTypeID = controllerTypeID;
            ControllerTypeName = controllerTypeName;
        }

        /// <summary>
        /// Определяет, соответствует ли текущий запрос <paramref name="request"/> со строкой адреса <paramref name="relativeURL"/> данному типу контроллера.
        /// </summary>
        public abstract bool IsThisRequestIsThisControllerType(HttpRequestBase request, string relativeURL);

        /// <summary>
        /// Возвращает ошибку, когда тип контроллера для модуля <paramref name="module"/> для запроса со значениями <paramref name="routeValues"/> не был найден.
        /// </summary>
        public abstract string ErrorCannotFindControllerTypeSpecified(IModuleCore module, RouteValueDictionary routeValues);

        /// <summary>
        /// Проверяет, доступен ли вход в указанный тип контроллера для модуля <paramref name="module"/> для запроса со значениями <paramref name="routeValues"/>.
        /// </summary>
        public abstract bool CheckPermissions(IModuleCore module, RouteValueDictionary routeValues);

        /// <summary>
        /// Создает новый относительный url на основе переданных данных о модуле и методе.
        /// </summary>
        public abstract string CreateRelativeUrl(string moduleName, string actionName, string[] parameters = null);

        public string ControllerTypeName
        {
            get;
            private set;
        }

        public int ControllerTypeID
        {
            get;
            private set;
        }
    }
}
