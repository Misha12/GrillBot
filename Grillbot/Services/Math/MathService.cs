using Discord.WebSocket;
using Grillbot.Database.Entity.Math;
using Grillbot.Database.Repository;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Models.Config.Dynamic;
using Grillbot.Models.Math;
using Grillbot.Services.Initiable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private ILogger<MathService> Logger { get; }

        public MathService(IOptions<Configuration> config, IServiceProvider serviceProvider, ILogger<MathService> logger)
        {
            Config = config.Value;
            Sessions = new List<MathSession>();
            ServiceProvider = serviceProvider;
            Logger = logger;
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

            var user = (SocketGuildUser)message.Author;

            try
            {
                bool booster = Config.Discord.IsBooster(user.Roles);
                session = LockAndGetSession(input, booster);

                input = ("" + input).Trim(); // treatment against null values.

                if (string.IsNullOrEmpty(input))
                {
                    result = new MathCalcResult() { ErrorMessage = "Nelze spočítat prázdný výraz." };
                    return result;
                }

                if (input.Contains("nan", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = new MathCalcResult() { ErrorMessage = "NaN není platný vstup." };
                    return result;
                }

                var appPath = GetExecutablePath(user.Guild);

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

                    if(result == null)
                    {
                        result = new MathCalcResult
                        {
                            IsValid = false,
                            ErrorMessage = "Výpočetní jednotka nevrátila žádná data."
                        };
                    }

                    const string exceptionPrefix = "|EXCEPTION|";
                    if(!result.IsValid && result.ErrorMessage.StartsWith(exceptionPrefix))
                    {
                        var exception = result.ErrorMessage.Substring(exceptionPrefix.Length).Trim();
                        Logger.LogError(exception);
                        
                        var lines = exception.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        result.ErrorMessage = lines[0];
                    }

                    return result;
                }
            }
            finally
            {
                SaveAudit(user, message, session, result);
                ReleaseSession(session, result);
            }
        }

        private void SaveAudit(SocketGuildUser user, SocketUserMessage message, MathSession session, MathCalcResult result)
        {
            using var scope = ServiceProvider.CreateScope();
            using var auditService = scope.ServiceProvider.GetRequiredService<MathAuditService>();

            auditService.SaveItem(user, message.Channel, session, result);
        }

        private string GetExecutablePath(SocketGuild guild)
        {
            using var scope = ServiceProvider.CreateScope();
            using var repository = scope.ServiceProvider.GetRequiredService<ConfigRepository>();

            var configData = repository.FindConfig(guild.Id, "", "solve");
            var config = configData?.GetData<MathConfig>();

            if (config == null)
                throw new InvalidOperationException("Chybí konfigurace matematické služby. Nelze získat cestu k výpočetnímu programu.");

            return config.ProcessPath;
        }

        public async Task InitAsync() { }

        public void Init()
        {
            InitSessions();
        }
    }
}
