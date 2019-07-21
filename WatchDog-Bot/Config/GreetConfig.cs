using Microsoft.Extensions.Configuration;

namespace WatchDog_Bot.Config
{
    public class GreetConfig
    {
        public string Message { get; set; }
        public string AppendEmoji { get; set; }
        public string OutputMode { get; set; }

        public GreetConfig(IConfiguration config)
        {
            AppendEmoji = config["AppendEmoji"];
            Message = config["Message"];
            OutputMode = config["OutputMode"] ?? "text";
        }
    }
}
