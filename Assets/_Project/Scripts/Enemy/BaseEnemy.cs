using System;
using Assets.ScriptableObjects;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Systems;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Enemy
{
    public class BaseEnemy : MonoBehaviour, IMovable, IDamageable, IAttacker
    {
        [SerializeField] private HealthSystem _healthSystem;
        
        public IMoveController MoveController { get; private set; }
        public IAnimator Animator { get; private set; }
        public int CurrentHp => _healthSystem.CurrentHp;
        public int MaxHp => _healthSystem.MaxHp;
        public bool IsAlive => _healthSystem.IsAlive;
        public float Speed { get; private set; }
        public ITarget CurrentTarget { get; private set; }
        
        [Inject]
        public void Init(PlayerConfig config)
        {
            Speed = config.Speed;
            MoveController = GetComponent<IMoveController>();
            Animator = GetComponent<IAnimator>();
        }

        private void OnDestroy()
        { 
            
        }

        public void SetTarget(ITarget target)
        {
            CurrentTarget = target;
        }

        private void Update()
        {
            if (CurrentTarget == null || !CurrentTarget.IsAlive) return;

            float distance = Vector2.Distance(transform.position, CurrentTarget.Transform.position);

        }

        private void MoveTowardsTarget()
        {
            Vector2 direction = (CurrentTarget.Transform.position - transform.position).normalized;
            MoveController?.Move(direction, Speed);
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
    }
}
