using Assets.Scripts.Interfaces;
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
        private IInteractableService _interactableService;
        private IResourceService _playerResourceService;
        private GameObject _currentAppearance;

        public int Level { get; private set; } = 0;
        public int MaxLvl;

        [Inject]
        private void Init(IInteractableService interactableService, IResourceService referenceService)
        {
            _playerResourceService = referenceService;
            _interactableService = interactableService;

            _levels = _levelEntries.ToDictionary(entry => entry.Level, entry => entry);
            MaxLvl = _levels.Keys.Max();
            _buildPrice.ActivePrice(false);
            UpgradeToLevel(1);
        }

        public bool TryGetLevelData(int level, out BuildingLevelData data)
        {
            data = null;
            return _levels == null ? false : _levels.TryGetValue(level, out data);
        }

        public void UpgradeToLevel(int newLevel)
        {
            if (!TryGetLevelData(newLevel, out var data) || newLevel == Level || newLevel > MaxLvl) return;
            if (!IsMaxLevel()) _buildPrice.ActivePrice(false);

            Level = newLevel;
            SetAppearance(data.Appearance);
            Debug.Log($"Building upgraded to level {Level}");
        }

        private void SetAppearance(GameObject newAppearance)
        {
            if (_currentAppearance != null) _currentAppearance.SetActive(false);

            newAppearance.SetActive(true);
            _currentAppearance = newAppearance;
        }

        public void Interactable()
        {
            var spend = _playerResourceService.TrySpend(Enum.ResourceType.Gold, _levels[Level + 1].UpgradeCost);
            if (!IsMaxLevel() && spend) UpgradeToLevel(Level + 1);
        }

        private void PlayerTriggerCollider(bool value, Collider2D collision)
        {
            if (!IsPlayerCollider(collision) || IsMaxLevel()) return;

            if (value) _interactableService.Set(this);
            else _interactableService.Clear(this);

            _buildPrice.ActivePrice(value);
        }

        private void OnTriggerEnter2D(Collider2D collision) => PlayerTriggerCollider(true, collision);

        private void OnTriggerExit2D(Collider2D collision) => PlayerTriggerCollider(false, collision);

        private bool IsMaxLevel() => Level == MaxLvl;
        private bool IsPlayerCollider(Collider2D collider) => collider.CompareTag(Constants.Constants.PLAYER_TAG);
    }
}
