using Grillbot.Exceptions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Grillbot.Services.TempUnverify
{
    public class TempUnverifyReasonParser
    {
        public string Parse(string data)
        {
            AssertInput(() => !data.StartsWith("<@"));

            var reason = data.Split("<@", StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            AssertInput(() => !string.IsNullOrEmpty(reason));

            return reason;
        }

        private void AssertInput(Func<bool> condition)
        {
            if (condition()) return;

            throw new ValidationException("Nemůžu bezdůvodně odebrat přístup. Uveď důvod (`unverify {time} {reason} [{tags}]`)");
        }
    }
}
