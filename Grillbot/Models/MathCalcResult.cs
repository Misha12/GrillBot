using System;

namespace Grillbot.Models
{
    public class MathCalcResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public double Result { get; set; }
        public double ComputingTime { get; set; }
        public int AssingedComputingTime { get; set; }
        public string Mention { get; set; }

        public MathCalcResult() { }

        public MathCalcResult(string mention, string errorMessage, int assignedComputingTime)
        {
            Mention = mention;
            ErrorMessage = errorMessage;
            AssingedComputingTime = assignedComputingTime;
        }

        public MathCalcResult(string mention, double result, double computingTime, int assignedComputingTime)
        {
            Mention = mention;
            Result = result;
            IsValid = true;
            ComputingTime = computingTime;
            AssingedComputingTime = assignedComputingTime;
        }

        public string GetMention() => !string.IsNullOrEmpty(Mention) ? Mention : "";
        public string GetComputingTime() => ComputingTime < 1000.0 ? $"{ComputingTime}ms" : $"{ComputingTime / 1000.0}s";
        public string GetAssignedComputingTime() => AssingedComputingTime < 1000 ? $"{AssingedComputingTime}ms" : $"{AssingedComputingTime / 1000}s";
    }
}
