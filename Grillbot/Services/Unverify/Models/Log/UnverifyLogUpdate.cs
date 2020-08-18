using System;

namespace Grillbot.Services.Unverify.Models.Log
{
    public class UnverifyLogUpdate : UnverifyLogBase
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}
