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
        private BuildingUpgrade _upgrade;
        private GameObject _currentAppearance;

        protected BuildingConfig Config { get; private set; }

        public int Level => _upgrade == null ? -1 : _upgrade.Level;
        public int CurrentHp => _healthSystem.CurrentHp;
        public int MaxHp => _healthSystem.MaxHp;
        public bool IsAlive => _healthSystem.IsAlive;
        public Transform Transform => transform;
        public bool CanInteract => _upgrade != null && _upgrade.CanUpgrade;
        public bool HasBuildPrice => _buildPrice != null;
        public bool HasHealthSystem => _healthSystem != null;

        [Inject]
        private void Init(IInteractableService interactableService, IResourceService referenceService)
        {
            _playerResourceService = referenceService;
            _interactableService = interactableService;
        }

        public void Initialize(BuildingConfig config)
        {
            Config = config;
            _levels = config.Levels.ToDictionary(entry => entry.Level, entry => entry);
            int maxLevel = _levels.Keys.Max();
            int[] costs = Enumerable.Repeat(-1, maxLevel + 1).ToArray();
            foreach (BuildingLevelConfig level in _levels.Values) costs[level.Level] = level.UpgradeCost;
            _upgrade = new BuildingUpgrade(costs, cost => _playerResourceService.TrySpend(Enum.ResourceType.Gold, cost));
            SpawnAllAppearances();
            SetAppearance(0);
            _healthSystem?.Initialize(_levels[0].MaxHp);
            _buildPrice.ActivePrice(false);
        }

        private void SpawnAllAppearances()
        {
            _appearanceInstances = new Dictionary<int, GameObject>();
            foreach (BuildingLevelConfig level in _levels.Values)
            {
                if (level.AppearancePrefab != null)
                {
                    GameObject appearance = Instantiate(level.AppearancePrefab, transform);
                    appearance.SetActive(false);
                    _appearanceInstances[level.Level] = appearance;
                }
            }
        }

        private void SetAppearance(int level)
        {
            if (_currentAppearance != null) _currentAppearance.SetActive(false);

            if (_appearanceInstances.TryGetValue(level, out GameObject appearance))
            {
                appearance.SetActive(true);
                _currentAppearance = appearance;
            }
        }

        public void Interactable()
        {
            if (_upgrade == null || _upgrade.TryUpgrade() == false) return;

            SetAppearance(Level);
            _healthSystem?.Initialize(_levels[Level].MaxHp);

            if (CanInteract) _buildPrice.SetPrice(_upgrade.NextCost);
            else
            {
                _buildPrice.ActivePrice(false);
                _interactableService.Clear(this);
            }
        }

        public void TakeDamage(int amount)
        {
            _healthSystem.TakeDamage(amount);

            if (IsAlive == false)
            {
                Debug.Log($"{gameObject.name} destroyed!");
                Destroy(gameObject);
            }
        }

        public void SetSelected(bool selected)
        {
            if (selected && CanInteract) _buildPrice.SetPrice(_upgrade.NextCost);
            _buildPrice.ActivePrice(selected && CanInteract);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (IsPlayerCollider(collision)) _interactableService.Set(this);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (IsPlayerCollider(collision)) _interactableService.Clear(this);
        }

        private void OnDisable()
        {
            _interactableService?.Clear(this);
        }

        private bool IsPlayerCollider(Collider2D collider) => collider.CompareTag(Constants.Constants.PLAYER_TAG);
    }
}
