using Discord.WebSocket;
using Grillbot.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class MathCalculator
    {
        private IConfiguration Config { get; }

        public MathCalculator(IConfiguration config)
        {
            Config = config;
        }

        public MathCalcResult Solve(string input, SocketUserMessage message)
        {
            if (string.IsNullOrEmpty(input))
                return new MathCalcResult(message?.Author.Mention, "Nelze spočítat prázdný požadavek.");

            if (input.Contains("nan", StringComparison.InvariantCultureIgnoreCase))
                return new MathCalcResult(message?.Author.Mention, "Toho bys asi chtěl moc.");

            var appPath = Config["MethodsConfig:Math:ProcessPath"];
            var calcTime = Convert.ToInt32(Config["MethodsConfig:Math:ComputingTime"]);

            using (var process = new Process())
            {
                process.StartInfo.FileName = "dotnet";
                process.StartInfo.Arguments = $"{appPath} \"{input}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();
                
                if(!process.WaitForExit(calcTime))
                {
                    process.Kill();
                    return new MathCalcResult(message.Author.Mention, "Vypršel mi časový limit na výpočet příkladu.");
                }

                var output = process.StandardOutput.ReadToEnd();
                var data = JsonConvert.DeserializeObject<MathCalcResult>(output);

                data.Mention = message.Author.Mention;
                return data;
            }
        }
    }
}
