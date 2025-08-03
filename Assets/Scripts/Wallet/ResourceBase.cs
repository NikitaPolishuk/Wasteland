using Assets.Scripts.Enum;
using Assets.Scripts.Interfaces;
using System;

namespace Assets.Scripts.Wallet
{
    public abstract class ResourceBase : IResource
    {
        public abstract ResourceType Type { get; }

        public int Amount { get; protected set; }

        public virtual void Add(int value)
        {
            if (value < 0) throw new ArgumentException("Cannot add negative amount.");
            Amount += value;
        }

        public virtual bool TrySpend(int value)
        {
            if (value < 0 || value > Amount) return false;

            Amount -= value;
            return true;
        }
    }
}
