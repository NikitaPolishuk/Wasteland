using Assets.Scripts.Character;
using Assets.Scripts.Constants;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Wallet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Buildings
{
    public abstract class BaseBuilding : MonoBehaviour, IInteractable
    {
        [SerializeField] private List<BuildingLevelData> _levelEntries;
        [SerializeField] private BuildPrice _buildPrice;

        private Dictionary<int, BuildingLevelData> _levels;
        private InteractableService _interactableService;
        private PlayerResourceService _playerResourceService;

        public int Level { get; private set; } = 1;

        [Inject]
        private void Init(InteractableService interactableService, PlayerResourceService referenceService)
        {
            _playerResourceService = referenceService;
            _interactableService = interactableService;
        }

        private void Awake()
        {
            _levels = _levelEntries.ToDictionary(entry => entry.Level, entry => entry);
            UpgradeToLevel(Level);
            _buildPrice.ActivePrice(false);
        }

        public bool TryGetLevelData(int level, out BuildingLevelData data)
        {
            data = new BuildingLevelData();
            return _levels == null ? false : _levels.TryGetValue(level, out data);
        }

        public void UpgradeToLevel(int newLevel)
        {
            if (!TryGetLevelData(newLevel, out var data)) return;

            Level = newLevel;

            foreach (var item in _levelEntries)
            {
                item.Appearance.gameObject.SetActive(false);
            }

            data.Appearance.gameObject.SetActive(true);

            if(Level == _levels.Keys.Max())
            {
                _buildPrice.ActivePrice(false);
            }

            Debug.Log($"Building upgraded to level {Level}");
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag(Constants.Constants.PLAYER_TAG) || Level == _levels.Keys.Max()) return;
            _buildPrice.ActivePrice(true);
            _interactableService.Set(this);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.CompareTag(Constants.Constants.PLAYER_TAG) || Level == _levels.Keys.Max()) return;
            _buildPrice.ActivePrice(false);
            _interactableService.Clear(this);
        }

        public void Interactable()
        {
            var spend = _playerResourceService.TrySpend(Enum.ResourceType.Gold, _levels[Level + 1].UpgradeCost);
           if (Level != _levels.Keys.Max() && spend) UpgradeToLevel(Level+1);
        } 
    }
}
