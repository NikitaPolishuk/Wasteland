using Assets.Scripts.Enum;

namespace Assets.Scripts.Interfaces
{
    public interface IResource
    {
        ResourceType Type { get; }
        int Amount { get; }
        void Add(int value);
        bool TrySpend(int value);
    }
}

