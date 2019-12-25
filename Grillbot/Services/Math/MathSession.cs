using System;

namespace Grillbot.Services.Math
{
    public class MathSession
    {
        public int ID { get; set; }
        public string Expression { get; set; }
        public bool IsUsed => !string.IsNullOrEmpty(Expression);
        public bool ForBooster { get; set; }
        public int ComputingTime { get; set; }

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
            Expression = expression;
        }

        public void Release()
        {
            Expression = null;
        }
    }
}
