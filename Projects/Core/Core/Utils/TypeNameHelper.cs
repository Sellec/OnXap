using System;

namespace OnXap.Utils
{
    using Core.Modules;

    class TypeNameHelper
    {
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
