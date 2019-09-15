using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Config
{
    [AttributeUsage(AttributeTargets.Property)]
    public class StrictPrivateAttribute : Attribute
    {
    }
}
