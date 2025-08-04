using Assets.Scripts.Enum;
using System;

namespace Assets.Scripts.Interfaces
{
    public interface IResourceService
    {
        Action<ResourceType, int> ResourceChange { get; set; }

        void Add(ResourceType type, int amount);
        bool TrySpend(ResourceType type, int amount);
        int GetAmount(ResourceType type);
    }
}
