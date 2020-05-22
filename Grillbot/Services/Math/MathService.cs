using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Models.Config.Dynamic;
using Grillbot.Models.Math;
using Grillbot.Services.Initiable;
using Microsoft.Extensions.DependencyInjection;
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
        public List<MathSession> Sessions { get; }
        private static readonly object Locker = new object();
        private Configuration Config { get; }
        private IServiceProvider ServiceProvider { get; }

        public MathService(IOptions<Configuration> config, IServiceProvider serviceProvider)
        {
            Config = config.Value;
            Sessions = new List<MathSession>();
            ServiceProvider = serviceProvider;
        }

        private void InitSessions()
        {
            lock (Locker)
            {
                Sessions.Clear();

                const int sessionCount = 10; // 10 computing units (processes) for every group.
                const int calcTime = 10000; // 10 seconds. Booster have double time.
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
                    throw new InvalidOperationException("Aktuálně nejsou volné výpočetní jednotky. Zkus to později.");

                session.Use(expression);
                return session;
            }
        }

        private void ReleaseSession(MathSession session, MathCalcResult result)
        {
            if (session == null) return;

            lock (Locker)
            {
                session.Release(result);
            }
        }

        public MathCalcResult Solve(string input, SocketUserMessage message)
        {
            MathSession session = null;
            MathCalcResult result = null;

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

                using var scope = ServiceProvider.CreateScope();
                using var repository = scope.ServiceProvider.GetConfigRepository();
                var config = repository.FindConfig(user.Guild.Id, "", "solve");
                var configData = config.GetData<MathConfig>();

                var appPath = configData.ProcessPath;
                using var process = new Process();

                process.StartInfo.FileName = appPath;
                process.StartInfo.Arguments = $"\"{input}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();

                if (!process.WaitForExit(session.ComputingTime))
                {
                    process.Kill();

                    result = new MathCalcResult()
                    {
                        IsTimeout = true,
                        AssingedComputingTime = session.ComputingTime
                    };

                    return result;
                }
                else
                {
                    var output = process.StandardOutput.ReadToEnd();
                    result = JsonConvert.DeserializeObject<MathCalcResult>(output);
                    return result;
                }
            }
            finally
            {
                ReleaseSession(session, result);
            }
        }

        public async Task InitAsync() { }

        public void Init()
        {
            InitSessions();
        }
    }
}
