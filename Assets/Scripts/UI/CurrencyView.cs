using Assets.Scripts.Enum;
using Assets.Scripts.Interfaces;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class CurrencyView : MonoBehaviour, ICurrencyView

    {
        [SerializeField] private TMP_Text _currencyText;

        public ResourceType CurrencyType { get; private set; }

        public void UpdateCurrencyDisplay(int amount)
        {
            _currencyText.text = amount.ToString();
        }
    }
}
