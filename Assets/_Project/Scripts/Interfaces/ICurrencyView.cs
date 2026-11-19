using Assets.Scripts.Enum;
using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface ICurrencyView
    {
        ResourceType CurrencyType { get; }
        void UpdateCurrencyDisplay(int amount);
    }
}
