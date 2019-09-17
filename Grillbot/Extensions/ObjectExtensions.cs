using Grillbot.Services.Config;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Grillbot.Extensions
{
    public static class ObjectExtensions
    {
        public static object GetPropertyValue(this object obj, string route)
        {
            if (obj == null)
                return "Undefined object";

            var parts = route.Split('.');

            if (parts.Length == 1)
                return GetConcretePropertyValue(obj, obj.GetType().GetProperty(route));

            foreach(var part in parts)
            {
                var type = obj.GetType();

                var property = type.GetProperty(part);
                if (property == null)
                    return $"Part {part} is undefined.";

                obj = GetConcretePropertyValue(obj, property);

                if (obj is string s && s == "Private object")
                    return obj;
            }

            return obj;
        }

        private static object GetConcretePropertyValue(object obj, PropertyInfo property)
        {
            if (property.GetCustomAttribute<StrictPrivateAttribute>() != null)
                return "Private object";

            var value = property.GetValue(obj, null);

            if (value is string str)
                return str;

            if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                return string.Join(", ", (IEnumerable<string>)value);

            return value;
        }
    }
}
