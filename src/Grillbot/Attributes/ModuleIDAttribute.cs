using System;

namespace Grillbot.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ModuleIDAttribute : Attribute
    {
        public string ID { get; set; }

        public ModuleIDAttribute(string id)
        {
            ID = id;
        }
    }
}
