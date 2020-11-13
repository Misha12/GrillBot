
namespace Grillbot.Core.Math.Models
{
    public class MathCalcResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public double Result { get; set; }
        public double ComputingTime { get; set; }
        public int AssingedComputingTime { get; set; }
        public bool IsTimeout { get; set; }

        public string GetComputingTime()
        {
            return ComputingTime < 1000.0 ? $"{ComputingTime}ms" : $"{ComputingTime / 1000.0}s";
        }

        public string GetAssignedComputingTime()
        {
            return AssingedComputingTime < 1000 ? $"{AssingedComputingTime}ms" : $"{AssingedComputingTime / 1000}s";
        }

        public string Format()
        {
            if (IsTimeout)
                return "Timeout";

            if (!IsValid)
                return ErrorMessage;

            return $"OK ({GetComputingTime()})";
        }
    }
}
