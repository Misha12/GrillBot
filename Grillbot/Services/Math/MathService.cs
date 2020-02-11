using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Models;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Initiable;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Math
{
    public class MathService : IInitiable
    {
        private List<MathSession> Sessions { get; }
        private static readonly object Locker = new object();
        private Configuration Config { get; }

        public MathService(IOptions<Configuration> config)
        {
            Config = config.Value;
            Sessions = new List<MathSession>();
        }

        private void InitSessions()
        {
            lock (Locker)
            {
                Sessions.Clear();

                const int sessionCount = 10; // 10 computing units (processes) for every group.
                var calcTime = 10000; // 10 seconds. Booster have double time.
                Sessions.AddRange(Enumerable.Range(0, sessionCount).Select(i => new MathSession(i, calcTime, false))); // Basic
                Sessions.AddRange(Enumerable.Range(0, sessionCount).Select(i => new MathSession(i, calcTime, true))); // Server booster.
            }
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

            lock (Locker)
            {
                session.Release();
            }
        }

        public MathCalcResult Solve(string input, SocketUserMessage message)
        {
            MathSession session = null;

            try
            {
                var user = (SocketGuildUser)message.Author;
                bool booster = Config.Discord.IsBooster(user.Roles);
                session = LockAndGetSession(input, booster);

                input = ("" + input).Trim(); // treatment against null values.

                if (string.IsNullOrEmpty(input))
                {
                    return new MathCalcResult()
                    {
                        ErrorMessage = "Nelze spočítat prázdný výraz.",
                    };
                }

                if (input.Contains("nan", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new MathCalcResult()
                    {
                        ErrorMessage = "NaN není platný vstup."
                    };
                }

                using (var repository = new GrillBotRepository(Config))
                {
                    var config = repository.Config.FindConfig(user.Guild.Id, "", "solve");
                    var configData = config.GetData<MathConfig>();

                    var appPath = configData.ProcessPath;
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
                            return new MathCalcResult()
                            {
                                IsTimeout = true,
                                AssingedComputingTime = session.ComputingTime
                            };
                        }

                        var output = process.StandardOutput.ReadToEnd();
                        return JsonConvert.DeserializeObject<MathCalcResult>(output);
                    }
                }
            }
            finally
            {
                ReleaseSession(session);
            }
        }

        public async Task InitAsync() { }

        public void Init()
        {
            InitSessions();
        }
    }
}
