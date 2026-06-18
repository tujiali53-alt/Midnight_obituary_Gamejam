using System.Collections.Generic;
using MidnightObituary.Gameplay.Services;

namespace MidnightObituary.Tests.EditMode
{
    internal sealed class DeterministicRandomProvider : IRandomProvider
    {
        private readonly Queue<int> _values;

        public DeterministicRandomProvider(params int[] values)
        {
            _values = new Queue<int>(values);
        }

        public int RangeInclusive(int minInclusive, int maxInclusive)
        {
            int value = _values.Dequeue();
            if (value < minInclusive)
            {
                return minInclusive;
            }

            if (value > maxInclusive)
            {
                return maxInclusive;
            }

            return value;
        }
    }
}
