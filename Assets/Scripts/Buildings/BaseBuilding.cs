using Assets.ScriptableObjects.BuildingConfig;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Systems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Buildings
{
    public abstract class BaseBuilding : MonoBehaviour, IInteractable, IDamageable, ITarget
    {
        [SerializeField] private BuildPrice _buildPrice;
        [SerializeField] private HealthSystem _healthSystem;

        private Dictionary<int, BuildingLevelConfig> _levels;
        private Dictionary<int, GameObject> _appearanceInstances;
        private IInteractableService _interactableService;
        private IResourceService _playerResourceService;
        private GameObject _currentAppearance;

        public int Level { get; private set; } = -1;
        public int MaxLvl;
        public int CurrentHp => _healthSystem.CurrentHp;
        public int MaxHp => _healthSystem.MaxHp;
        public bool IsAlive => _healthSystem.IsAlive;
        public Transform Transform => transform;

        [Inject]
        private void Init(IInteractableService interactableService, IResourceService referenceService)
        {
            _playerResourceService = referenceService;
            _interactableService = interactableService;
        }

        public void Initialize(BuildingConfig config)
        {
            _levels = config.Levels.ToDictionary(entry => entry.Level, entry => entry);
            SpawnAllAppearances();
            MaxLvl = _levels.Keys.Max();
            _buildPrice.ActivePrice(false);
            UpgradeToLevel(0);
        }

        private void SpawnAllAppearances()
        {
            _appearanceInstances = new Dictionary<int, GameObject>();
            foreach (var level in _levels.Values)
            {
                if (level.AppearancePrefab != null)
                {
                    var appearance = Instantiate(level.AppearancePrefab, transform);
                    appearance.SetActive(false);
                    _appearanceInstances[level.Level] = appearance;
                }
            }
        }

        public bool TryGetLevelData(int level, out BuildingLevelConfig data)
        {
            data = null;
            return _levels == null ? false : _levels.TryGetValue(level, out data);
        }

        public void UpgradeToLevel(int newLevel)
        {
            if (!CanUpgradeToLevel(newLevel, out var data)) return;

            ApplyUpgrade(newLevel, data);
        }

        private bool CanUpgradeToLevel(int newLevel, out BuildingLevelConfig data)
        {
            data = null;

            if (newLevel <= Level) return false;
            if (newLevel > MaxLvl) return false;
            if (!TryGetLevelData(newLevel, out data)) return false;

            return true;
        }

        private void ApplyUpgrade(int newLevel, BuildingLevelConfig data)
        {
            Level = newLevel;
            SetAppearance(newLevel);
            _healthSystem?.Initialize(data.MaxHp);
            
            if (IsMaxLevel())
            {
                _buildPrice.ActivePrice(false);
                _interactableService.Clear(this);
            }

            Debug.Log($"Building upgraded to level {Level}");
        }

        private void SetAppearance(int level)
        {
            if (_currentAppearance != null) _currentAppearance.SetActive(false);

            if (_appearanceInstances.TryGetValue(level, out var appearance))
            {
                appearance.SetActive(true);
                _currentAppearance = appearance;
            }
        }

        public void Interactable()
        {
            var spend = _playerResourceService.TrySpend(Enum.ResourceType.Gold, _levels[Level + 1].UpgradeCost);
            if (!IsMaxLevel() && spend) UpgradeToLevel(Level + 1);
        }

        public void TakeDamage(int amount)
        {
            _healthSystem.TakeDamage(amount);

            if (!IsAlive)
            {
                Debug.Log($"{gameObject.name} destroyed!");
                Destroy(gameObject);
            }
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