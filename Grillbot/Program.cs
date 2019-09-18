using Grillbot.Services.Config.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Grillbot
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (!ExistsConfig()) GenerateConfig();

            WebHost.CreateDefaultBuilder(args)
                .UseStartup<AppStartup>()
                .ConfigureLogging(o => o.SetMinimumLevel(LogLevel.Warning))
                .Build()
                .Run();
        }

        public const string AppSettingsFilename = "appsettings.json";

        private static bool ExistsConfig() => File.Exists(AppSettingsFilename);

        private static void GenerateConfig()
        {
            Console.WriteLine("appsettings.json file not found.");

            var discordToken = GetValueFromInput("Insert token of discord development server: ");
            var databaseConnectionString = GetValueFromInput("Insert database connection string: ");
            var mathModulePath = GetValueFromInput("Insert GrillBotMath.dll path, optional: ", true);

            var defaultConfig = Configuration.GenerateDefault(discordToken, databaseConnectionString, mathModulePath);

            var jsonSettings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.Indented
            };

            var json = JsonConvert.SerializeObject(defaultConfig, jsonSettings);
            Console.WriteLine("Default config generated. Starting.");

            File.WriteAllText(AppSettingsFilename, json);
        }

        private static string GetValueFromInput(string message, bool optional = false)
        {
            while (true)
            {
                Console.Write(message);
                var result = Console.ReadLine();

                if (string.IsNullOrEmpty(result) && !optional)
                {
                    Console.WriteLine("Invalid input, try again.");
                    continue;
                }

                return result;
            }
        }
    }

}
