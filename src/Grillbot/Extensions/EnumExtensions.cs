using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Grillbot.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            var type = value.GetType();
            var members = type.GetMember(value.ToString());

            if (members == null || members.Length == 0)
                return value.ToString();

            return members.First()
                .GetCustomAttribute<DisplayAttribute>(false)?
                .Name ?? value.ToString();
        }
    }
}
