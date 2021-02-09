using System.Reflection;

namespace Grillbot.Extensions
{
    public static class ObjectExtensions
    {
        public static bool HasProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }
    }
}
