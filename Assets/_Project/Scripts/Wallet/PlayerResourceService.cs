using Assets.Scripts.Enum;
using Assets.Scripts.Interfaces;
using System;
using Zenject;

namespace Assets.Scripts.Wallet
{
    public class PlayerResourceService : IResourceService, IDisposable
    {
        private ResourceWallet _wallet;
        public Action<ResourceType, int> ResourceChange { get; set; }

        [Inject]
        public PlayerResourceService(ResourceWallet wallet)
        {
            _wallet = wallet;

            var gold = new GoldResource();
            _wallet.RegisterResource(gold);
            Add(ResourceType.Gold, 30);
        }

        public void Add(ResourceType type, int amount)
        {
            _wallet.Add(type, amount);
            ResourceChange?.Invoke(type, GetAmount(type));
        }

        public bool TrySpend(ResourceType type, int amount)
        {
            var final = _wallet.TrySpend(type, amount);
            ResourceChange?.Invoke(type, GetAmount(type));
            return final;
        }

        public int GetAmount(ResourceType type) => _wallet.GetAmount(type);

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
