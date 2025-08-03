using Assets.Scripts.Enum;
using System;
using Zenject;

namespace Assets.Scripts.Wallet
{
    public class PlayerResourceService : IDisposable
    {
        private ResourceWallet _wallet;

        [Inject]
        public PlayerResourceService(ResourceWallet wallet)
        {
            _wallet = wallet;

            var gold = new GoldResource();
            gold.Add(50);
            _wallet.RegisterResource(gold);
        }

        public void Add(ResourceType type, int amount) => _wallet.Add(type, amount);
        public bool TrySpend(ResourceType type, int amount) => _wallet.TrySpend(type, amount);
        public int GetAmount(ResourceType type) => _wallet.GetAmount(type);

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
