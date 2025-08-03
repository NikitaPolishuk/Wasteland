using Assets.Scripts.Enum;
using Assets.Scripts.Interfaces;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Wallet
{
    public class ResourceWallet 
    {
        private readonly Dictionary<ResourceType, IResource> _resources = new();

        public void RegisterResource(IResource resource)
        {
            if (_resources.ContainsKey(resource.Type)) throw new InvalidOperationException($"Resource {resource.Type} already registered.");
            _resources[resource.Type] = resource;
        }

        public IResource GetResource(ResourceType type)
        {
            if (!_resources.TryGetValue(type, out var resource)) throw new KeyNotFoundException($"Resource {type} not found.");
            return resource;
        }

        public int GetAmount(ResourceType type) => GetResource(type).Amount;
        public void Add(ResourceType type, int value) => GetResource(type).Add(value);
        public bool TrySpend(ResourceType type, int value) => GetResource(type).TrySpend(value);
    }
}
