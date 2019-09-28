﻿using Grillbot.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.Config.Models
{
    public class Configuration
    {
        public string AllowedHosts { get; set; }
        public string CommandPrefix { get; set; }

        [StrictPrivate]
        public string Database { get; set; }

        public int EmoteChain_CheckLastCount { get; set; }
        public DiscordConfig Discord { get; set; }
        public BotLogConfig Log { get; set; }
        public MethodsConfig MethodsConfig { get; set; }

        public List<string> Administrators { get; set; }

        public Configuration()
        {
            Administrators = new List<string>();
        }

        public bool IsUserBotAdmin(ulong id) => Administrators.Any(o => o == id.ToString());

        public string GetValue(string route) => this.GetPropertyValue(route)?.ToString();

        public static Configuration GenerateDefault(string token, string connectionString, string mathDllPath)
        {
            const string devRole = "dev";

            return new Configuration()
            {
                Administrators = new List<string>(),
                AllowedHosts = "*",
                CommandPrefix = "!",
                Database = connectionString,
                Discord = new DiscordConfig()
                {
                    Activity = "Help is !grillhelp",
                    LoggerRoomID = "",
                    Token = token,
                    UserJoinedMessage = "Vítej"
                },
                EmoteChain_CheckLastCount = 5,
                Log = new BotLogConfig() { LogRoomID = "" },
                MethodsConfig = new MethodsConfig()
                {
                    AutoReply = new AutoReplyConfig() { Permissions = new PermissionsConfig() { RequiredRoles = new List<string>() { devRole } } },
                    Channelboard = new ChannelboardConfig()
                    {
                        Permissions = new PermissionsConfig() { RequiredRoles = new List<string>() { devRole } },
                        WebTokenValidMinutes = 60,
                        WebUrl = "http://localhost:4200/channelboard?token={0}"
                    },
                    EmoteManager = new EmoteManagerConfig() { Permissions = new PermissionsConfig() { RequiredRoles = new List<string>() { devRole } } },
                    Greeting = new GreetingConfig()
                    {
                        Permissions = new PermissionsConfig() { RequiredRoles = new List<string>() { devRole } },
                        MessageTemplate = "Ahoj, {person}",
                        OutputMode = GreetingOutputModes.Text
                    },
                    GrillStatus = new GrillStatusConfig() { Permissions = new PermissionsConfig() { RequiredRoles = new List<string>() { devRole } } },
                    Help = new HelpConfig() { Permissions = new PermissionsConfig() { RequiredRoles = new List<string>() { devRole } } },
                    Math = new MathConfig()
                    {
                        Permissions = new PermissionsConfig() { RequiredRoles = new List<string>() { devRole } },
                        ComputingTime = 100000,
                        ProcessPath = mathDllPath
                    },
                    MemeImages = new MemeImagesConfig()
                    {
                        Permissions = new PermissionsConfig() { RequiredRoles = new List<string>() { devRole } },
                        AllowedImageTypes = new List<string>() { ".jpg", ".png", ".gif" },
                        NotNudesDataPath = "",
                        NudesDataPath = ""
                    },
                    ModifyConfig = new ModifyConfigConfig() { Permissions = new PermissionsConfig() { RequiredRoles = new List<string>() { devRole } } },
                    RoleManager = new RoleManagerConfig() { Permissions = new PermissionsConfig() { RequiredRoles = new List<string>() { devRole } } },
                    TeamSearch = new TeamSearchConfig()
                    {
                        GeneralCategoryID = 0,
                        Permissions = new PermissionsConfig() { RequiredRoles = new List<string>() { devRole } }
                    },
                    TempUnverify = new TempUnverifyConfig() { Permissions = new PermissionsConfig() { RequiredRoles = new List<string>() { devRole } } }
                }
            };
        }
    }
}
