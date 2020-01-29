using System;

namespace OnXap.Utils
{
    using Core.Modules;

    /// <summary>
    /// Вспомогательные методы для работы с типами.
    /// </summary>
    public static class TypeNameHelper
    {
        /// <summary>
        /// Очищает полное имя (с именем сборки) указанного типа <paramref name="type"/>, удаляя версию сборки и язык.
        /// </summary>
        /// <remarks>Подходит для сохранения имени типа в базе данных для случаев, например, когда может меняться культура или generic-тип оборачивает тип из сторонней сборки.</remarks>
        public static string GetFullNameCleared(Type type)
        {
            var fullName = type.FullName;
            if (type.IsConstructedGenericType && type.Assembly == typeof(ModuleCore).Assembly)
            {
                if (type.GenericTypeArguments.Length == 1)
                {
                    var assemblyName = type.GenericTypeArguments[0].Assembly.GetName();
                    fullName = fullName.Replace(", Version=" + assemblyName.Version.ToString(), "");
                    fullName = fullName.Replace(", Culture=" + (string.IsNullOrEmpty(assemblyName.CultureName) ? "neutral" : assemblyName.CultureName), "");
                }
            }

            return fullName;
        }
    }
}
