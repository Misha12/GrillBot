using Grillbot.Services.Config.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Config
{
    public class OptionsWriter
    {
        private IOptions<Configuration> Options { get; }

        public OptionsWriter(IOptions<Configuration> options)
        {
            Options = options;
        }

        public void UpdateOptions(Action<Configuration> applyChanges)
        {
            applyChanges(Options.Value);

            var newJson = JsonConvert.SerializeObject(Options.Value);
            File.Copy("appsettings.json", $"appsettings.old_{DateTime.Now.ToString("yyMMdd_HHmmss")}.json");
            File.WriteAllText("appsettings.json", newJson);
        }
    }
}
