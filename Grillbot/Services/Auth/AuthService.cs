using System;
using System.Collections.Generic;

namespace Grillbot.Services.Auth
{
    public class AuthService
    {
        public List<AuthToken> Tokens { get; }

        public AuthService()
        {
            Tokens = new List<AuthToken>();
        }

        public void AddToken(string token, int expiresAfter)
        {
            if (Tokens.Exists(o => o.Token == token))
                throw new ArgumentException("token_exists");

            var authToken = new AuthToken(token, expiresAfter);
            Tokens.Add(authToken);
        }

        public bool IsTokenValid(string token)
        {
            var tokenItem = Tokens.Find(o => o.Token == token);
            return tokenItem?.IsValid() == true;
        }
    }
}
