using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SiteSearch.Core.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static bool HasAttribute<T>(this PropertyInfo propertyInfo) where T : Attribute
        {
            return propertyInfo.GetCustomAttributes<T>().Any();
        }

        public static bool IsPropertyACollection(this PropertyInfo property)
        {
            return
                property.PropertyType.FullName != typeof(string).FullName &&
                property.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null;
        }
    }
}
