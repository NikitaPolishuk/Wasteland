using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Systems
{
    public class HealthSystem : MonoBehaviour
    {
        [SerializeField] private int _maxHp = 100;
        [SerializeField] private int _currentHp;
        [SerializeField] private Slider _healthSlider;

        [SerializeField] private UnityEvent<int> _onDamageTaken;
        [SerializeField] private UnityEvent _onDeath;

        public int CurrentHp => _currentHp;
        public int MaxHp => _maxHp;
        public bool IsAlive => _currentHp > 0;

        private void Awake()
        {
            _currentHp = _maxHp;
            UpdateSlider();
        }

        public void Initialize(int maxHp)
        {
            _maxHp = maxHp;
            _currentHp = maxHp;
            UpdateSlider();
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;

            _currentHp = Mathf.Max(0, _currentHp - amount);
            UpdateSlider();
            _onDamageTaken?.Invoke(amount);

            if (!IsAlive)
            {
                _onDeath?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (!IsAlive) return;

            _currentHp = Mathf.Min(_maxHp, _currentHp + amount);
            UpdateSlider();
        }

        private void UpdateSlider()
        {
            if (_healthSlider != null)
            {
                _healthSlider.gameObject.SetActive(_currentHp > 0);
                _healthSlider.maxValue = _maxHp;
                _healthSlider.value = _currentHp;
            }
        }
    }
}