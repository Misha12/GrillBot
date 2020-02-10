using System;
using System.Collections.Generic;

namespace Grillbot.Services.Config.Models
{
    public class TempUnverifyConfig
    {
        public string MainAdminID { get; set; }

        public ulong MainAdminSnowflake
        {
            get => Convert.ToUInt64(MainAdminID);
            set => MainAdminID = value.ToString();
        }

        public List<string> PreprocessRemoveAccess { get; set; }

        public TempUnverifyConfig()
        {
            PreprocessRemoveAccess = new List<string>();
        }
    }
}
