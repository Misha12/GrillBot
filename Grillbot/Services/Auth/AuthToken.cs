using System;

namespace Grillbot.Services.Auth
{
    public class AuthToken
    {
        public string Token { get; set; }
        public DateTime ValidTo { get; set; }

        public bool IsValid() => DateTime.Now < ValidTo;

        public AuthToken(string token, int expiredAfter)
        {
            Token = token;
            ValidTo = DateTime.Now.AddSeconds(expiredAfter);
        }
    }
}
