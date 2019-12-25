using Discord.WebSocket;
using Grillbot.Models;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Grillbot.Services.Math
{
    public class MathService
    {
        private List<MathSession> Sessions { get; }
        private static readonly object Locker = new object();

        private Configuration Config { get; }

        public MathService(IOptions<Configuration> config)
        {
            Config = config.Value;
            Sessions = new List<MathSession>();

            const int sessionCount = 10; // 10 computing units (processes) for every group.
            var calcTime = Config.MethodsConfig.Math.ComputingTime;
            Sessions.AddRange(Enumerable.Range(0, sessionCount).Select(i => new MathSession(i, calcTime, false))); // Basic
            Sessions.AddRange(Enumerable.Range(0, sessionCount).Select(i => new MathSession(i, calcTime, true))); // Server booster.
        }

        private MathSession LockAndGetSession(string expression, bool booster)
        {
            lock (Locker)
            {
                var session = Sessions.Find(o => !o.IsUsed && o.ForBooster == booster);

                if (session == null)
                    throw new ArgumentException("Aktuálně nejsou volné výpočetní jednotky. Zkus to později.");

                session.Use(expression);
                return session;
            }
        }

        private void ReleaseSession(MathSession session)
        {
            if (session == null) return;

            lock(Locker)
            {
                session.Release();
            }
        }

        public MathCalcResult Solve(string input, SocketUserMessage message)
        {
            MathSession session = null;

            try
            {
                bool booster = message.Author is SocketGuildUser user && Config.Discord.IsBooster(user.Roles);
                session = LockAndGetSession(input, booster);

                input = ("" + input).Trim(); // treatment against null values.

                if (string.IsNullOrEmpty(input))
                    return new MathCalcResult(message?.Author.Mention, "Nelze spočítat prázdný výraz.", session.ComputingTime);

                if (input.Contains("nan", StringComparison.InvariantCultureIgnoreCase))
                    return new MathCalcResult(message?.Author.Mention, "NaN není platný vstup.", session.ComputingTime);

                var appPath = Config.MethodsConfig.Math.ProcessPath;
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "dotnet";
                    process.StartInfo.Arguments = $"{appPath} \"{input}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;

                    process.Start();

                    if (!process.WaitForExit(session.ComputingTime))
                    {
                        process.Kill();
                        return new MathCalcResult(message?.Author?.Mention, $"Vypršel mi časový limit na výpočet příkladu.", session.ComputingTime);
                    }

                    var output = process.StandardOutput.ReadToEnd();
                    var data = JsonConvert.DeserializeObject<MathCalcResult>(output);

                    data.Mention = message.Author.Mention;
                    data.AssingedComputingTime = session.ComputingTime;
                    return data;
                }
            }
            finally
            {
                ReleaseSession(session);
            }
        }
    }
}
