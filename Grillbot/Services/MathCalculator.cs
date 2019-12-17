using Discord.WebSocket;
using Grillbot.Models;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Grillbot.Services
{
    public class MathCalculator
    {
        private Configuration Config { get; }

        public MathCalculator(IOptions<Configuration> config)
        {
            Config = config.Value;
        }

        public MathCalcResult Solve(string input, SocketUserMessage message)
        {
            if (string.IsNullOrEmpty(input))
                return new MathCalcResult(message?.Author.Mention, "Nelze spočítat prázdný požadavek.");

            if (input.Contains("nan", StringComparison.InvariantCultureIgnoreCase))
                return new MathCalcResult(message?.Author.Mention, "Toho bys asi chtěl moc.");

            var appPath = Config.MethodsConfig.Math.ProcessPath;
            var calcTime = Config.MethodsConfig.Math.ComputingTime;

            if (message.Author is SocketGuildUser user && Config.Discord.IsBooster(user.Roles))
                calcTime *= 2; // Double time for server boosters. Because boosters are great.

            using (var process = new Process())
            {
                process.StartInfo.FileName = "dotnet";
                process.StartInfo.Arguments = $"{appPath} \"{input}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();

                if (!process.WaitForExit(calcTime))
                {
                    process.Kill();
                    return new MathCalcResult(message?.Author?.Mention, "Vypršel mi časový limit na výpočet příkladu.");
                }

                var output = process.StandardOutput.ReadToEnd();
                var data = JsonConvert.DeserializeObject<MathCalcResult>(output);

                data.Mention = message.Author.Mention;
                return data;
            }
        }
    }
}
