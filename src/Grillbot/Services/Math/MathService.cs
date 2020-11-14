using Discord.WebSocket;
using Grillbot.Models.Math;
using Grillbot.Services.Config;
using Grillbot.Services.Initiable;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Math
{
    public class MathService : IInitiable
    {
        public List<MathSession> Sessions { get; }
        private static readonly object Locker = new object();
        private ILogger<MathService> Logger { get; }
        private ConfigurationService ConfigurationService { get; }

        public MathService(ILogger<MathService> logger, ConfigurationService configurationService)
        {
            Sessions = new List<MathSession>();
            Logger = logger;
            ConfigurationService = configurationService;
        }

        private void InitSessions()
        {
            lock (Locker)
            {
                Sessions.Clear();

                const int sessionCount = 5; // 10 computing units (processes) for every group.
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

        private static void ReleaseSession(MathSession session, MathCalcResult result)
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

            var boosterRoleId = ConfigurationService.GetValue(Enums.GlobalConfigItems.ServerBoosterRoleId);

            try
            {
                bool booster = !string.IsNullOrEmpty(boosterRoleId) && user.Roles.Any(o => o.Id == Convert.ToUInt64(boosterRoleId));
                session = LockAndGetSession(input, booster);

                input = ("" + input).Trim(); // treatment against null values.

                var parser = new ExpressionParser(input);

                if (parser.Empty)
                {
                    result = new MathCalcResult() { ErrorMessage = "Nelze spočítat prázdný výraz." };
                    return result;
                }

                if (!parser.IsValid)
                {
                    result = new MathCalcResult() { ErrorMessage = string.Join(Environment.NewLine, parser.Errors) };
                    return result;
                }

                try
                {
                    var task = Task.Run(() =>
                    {
                        return new MathCalcResult()
                        {
                            IsValid = true,
                            Result = parser.Expression.calculate(),
                            ComputingTime = parser.Expression.getComputingTime() * 1000
                        };
                    });

                    if (!task.Wait(session.ComputingTime))
                    {
                        try
                        {
                            task.Dispose();
                        }
                        catch (Exception) { /* This exception we can ignore. */ }

                        result = new MathCalcResult()
                        {
                            IsTimeout = true,
                            AssingedComputingTime = session.ComputingTime
                        };

                        return result;
                    }
                    else
                    {
                        result = task.Result;
                        return result;
                  }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "");

                    result = new MathCalcResult()
                    {
                        ErrorMessage = ex.Message
                    };

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
