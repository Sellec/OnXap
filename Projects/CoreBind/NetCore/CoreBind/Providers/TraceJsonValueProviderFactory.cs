﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Web.Mvc.Properties;
using System.Web.Script.Serialization;

using System.Web.Mvc;

namespace OnXap.CoreBind.Providers
{
    public sealed class TraceJsonValueProviderFactory : ValueProviderFactory
    {
        private static void AddToBackingStore(EntryLimitedDictionary backingStore, string prefix, object value)
        {
            if (!string.IsNullOrEmpty(prefix)) backingStore.Add(prefix, value);

            var d = value as IDictionary<string, object>;
            if (d != null)
            {
                foreach (var entry in d)
                {
                    AddToBackingStore(backingStore, MakePropertyKey(prefix, entry.Key), entry.Value);
                }
                return;
            }

            var l = value as IList;
            if (l != null)
            {
                for (int i = 0; i < l.Count; i++)
                {
                    AddToBackingStore(backingStore, MakeArrayKey(prefix, i), l[i]);
                }
                return;
            }

            // primitive
            //backingStore.Add(prefix, value);
        }

        private static object GetDeserializedObject(ControllerContext controllerContext)
        {
            if (!controllerContext.HttpContext.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            {
                // not JSON request
                return null;
            }

            var reader = new StreamReader(controllerContext.HttpContext.Request.InputStream);
            string bodyText = reader.ReadToEnd();
            if (String.IsNullOrEmpty(bodyText))
            {
                // no JSON data
                return null;
            }

            var serializer = new JavaScriptSerializer();
            object jsonData = serializer.DeserializeObject(bodyText);
            return jsonData;
        }

        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            var jsonData = GetDeserializedObject(controllerContext);
            if (jsonData == null)
            {
                return null;
            }

            var backingStore = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var backingStoreWrapper = new EntryLimitedDictionary(backingStore);
            AddToBackingStore(backingStoreWrapper, String.Empty, jsonData);
            return new TraceJsonValueProvider<object>(backingStore, CultureInfo.CurrentCulture);
        }

        private static string MakeArrayKey(string prefix, int index)
        {
            return prefix + "[" + index.ToString(CultureInfo.InvariantCulture) + "]";
        }

        private static string MakePropertyKey(string prefix, string propertyName)
        {
            return (String.IsNullOrEmpty(prefix)) ? propertyName : prefix + "." + propertyName;
        }

        private class EntryLimitedDictionary
        {
            private static int _maximumDepth = GetMaximumDepth();
            private readonly IDictionary<string, object> _innerDictionary;
            private int _itemCount = 0;

            public EntryLimitedDictionary(IDictionary<string, object> innerDictionary)
            {
                _innerDictionary = innerDictionary;
            }

            public void Add(string key, object value)
            {
                if (++_itemCount > _maximumDepth)
                {
                 //   throw new InvalidOperationException("Запрос слишком велик!");// MvcResources.JsonValueProviderFactory_RequestTooLarge);
                }

                _innerDictionary.Add(key, value);
            }

            private static int GetMaximumDepth()
            {
                var appSettings = ConfigurationManager.AppSettings;
                if (appSettings != null)
                {
                    string[] valueArray = appSettings.GetValues("aspnet:MaxJsonDeserializerMembers");
                    if (valueArray != null && valueArray.Length > 0)
                    {
                        int result;
                        if (Int32.TryParse(valueArray[0], out result))
                        {
                            return result;
                        }
                    }
                }

                return 1000; // Fallback default
            }
        }
    }

}
