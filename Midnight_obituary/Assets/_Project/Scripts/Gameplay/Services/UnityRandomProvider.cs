using UnityEngine;

namespace MidnightObituary.Gameplay.Services
{
    public sealed class UnityRandomProvider : IRandomProvider
    {
        public int RangeInclusive(int minInclusive, int maxInclusive)
        {
            return Random.Range(minInclusive, maxInclusive + 1);
        }
    }
}
