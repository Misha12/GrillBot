using System;
using System.Collections.Generic;
using System.Text;

namespace GrilBot.Helpers
{
    public static class StringHelper
    {
        public static string CreateRandomString(int length)
        {
            if (length == 0) return "";

            const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var str = new StringBuilder();
            var random = new Random();

            int randomValue;
            for (int i = 0; i < length; i++)
            {
                randomValue = random.Next(0, alphabet.Length);
                str.Append(alphabet[randomValue]);
            }

            return str.ToString();

        }
    }
}
