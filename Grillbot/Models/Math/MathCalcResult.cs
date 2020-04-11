
namespace Grillbot.Models.Math
{
    public class MathCalcResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public double Result { get; set; }
        public double ComputingTime { get; set; }
        public int AssingedComputingTime { get; set; }
        public bool IsTimeout { get; set; }

        public string GetComputingTime() => ComputingTime < 1000.0 ? $"{ComputingTime}ms" : $"{ComputingTime / 1000.0}s";
        public string GetAssignedComputingTime() => AssingedComputingTime < 1000 ? $"{AssingedComputingTime}ms" : $"{AssingedComputingTime / 1000}s";
    }
}
