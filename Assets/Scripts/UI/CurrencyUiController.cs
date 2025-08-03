using Assets.Scripts.Enum;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Wallet;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Assets.Scripts.UI
{
    public class CurrencyUiController
    {
        private readonly Dictionary<ResourceType, ICurrencyView> _views;
        private readonly PlayerResourceService _resourceService;

        [Inject]
        public CurrencyUiController(List<ICurrencyView> views, PlayerResourceService playerResourceService)
        {
            _resourceService = playerResourceService;
            _resourceService.ResourceChange += ResourceChangeHandler;
            _views = views.ToDictionary(v => v.CurrencyType, v => v);

            UpdateAllCurrenct();
        }

        private void UpdateAllCurrenct()
        {
            foreach (var pair in _views)
            {
                int amount = _resourceService.GetAmount(pair.Key);
                pair.Value.UpdateCurrencyDisplay(amount);
            }
        }

        private void ResourceChangeHandler(ResourceType resourceType, int value)
        {
            _views[resourceType].UpdateCurrencyDisplay(value);
        }

    }
}
