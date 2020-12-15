using Grillbot.Extensions;
using System;
using System.Text;

namespace Grillbot.Services
{
    public class ProgressBar
    {
        private const int TotalCellsCount = 50;
        private double _value;

        public double Value
        {
            get => _value;
            set => _value = value > 100.0D ? 100.0D : value;
        }

        public override string ToString()
        {
            var builder = new StringBuilder()
                .Append('[');

            var nonEmptyCellsCount = Convert.ToInt32(System.Math.Floor((TotalCellsCount / 100.0) * Value));
            var emptyCellsCount = TotalCellsCount - nonEmptyCellsCount;

            builder.Append("\\|".Repeat(nonEmptyCellsCount));
            builder.Append(new string(' ', emptyCellsCount));
            builder.Append(']');

            return builder.ToString();
        }
    }
}
