using System;

namespace Grillbot.Services.Config.Models
{
    public class TempUnverifyConfig : MethodConfigBase
    {
        public string MainAdminID { get; set; }

        public ulong MainAdminSnowflake
        {
            get => Convert.ToUInt64(MainAdminID);
            set => MainAdminID = value.ToString();
        }
    }
}
