namespace Grillbot.Core.Math.Models
{
    public class MathSession
    {
        public int ID { get; set; }
        public string Expression { get; set; }
        public bool IsUsed => !string.IsNullOrEmpty(Expression);
        public bool ForBooster { get; set; }
        public int ComputingTime { get; set; }
        public MathCalcResult LastResult { get; set; }
        public int UsedCount { get; set; }

        public MathSession(int id, int computingTime, bool forBooster)
        {
            ID = id;
            ForBooster = forBooster;
            ComputingTime = computingTime;

            if (forBooster)
                ComputingTime *= 2; // Double time for boosters. Because boosters are great.
        }

        public void Use(string expression)
        {
            UsedCount++;
            Expression = expression;
        }

        public void Release(MathCalcResult lastResult)
        {
            Expression = null;
            LastResult = lastResult;
        }
    }
}
