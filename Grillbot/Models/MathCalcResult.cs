using System;

namespace Grillbot.Models
{
    public class MathCalcResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public double Result { get; set; }
        public double ComputingTime { get; set; }
        public string Mention { get; set; }

        public MathCalcResult() { }

        public MathCalcResult(string mention, string errorMessage)
        {
            Mention = mention;
            ErrorMessage = errorMessage;
        }

        public MathCalcResult(string mention, double result, double computingTime)
        {
            Mention = mention;
            Result = result;
            IsValid = true;
            ComputingTime = computingTime;
        }

        public string GetMention() => !string.IsNullOrEmpty(Mention) ? Mention : "";
    }
}
