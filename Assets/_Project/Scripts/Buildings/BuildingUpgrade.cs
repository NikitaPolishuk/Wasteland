using System;

namespace Assets.Scripts.Buildings
{
    public sealed class BuildingUpgrade
    {
        private readonly int[] _costs;
        private readonly Func<int, bool> _trySpend;

        public BuildingUpgrade(int[] costs, Func<int, bool> trySpend)
        {
            _costs = (int[])costs.Clone();
            _trySpend = trySpend;
        }

        public int Level { get; private set; }
        public bool CanUpgrade => Level + 1 < _costs.Length && _costs[Level + 1] >= 0;
        public int NextCost => CanUpgrade ? _costs[Level + 1] : -1;

        public bool TryUpgrade()
        {
            if (CanUpgrade == false || _trySpend(NextCost) == false) return false;
            Level++;
            return true;
        }
    }
}
