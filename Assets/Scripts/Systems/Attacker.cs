using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Systems
{
    public class Attacker : MonoBehaviour, IAttacker
    {
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _attackCooldown = 1f;
        [SerializeField] private int _damage = 10;

        public ITarget CurrentTarget { get; private set; }

        private IMoveController _moveController;
        private float _speed;
        private float _attackTimer;

        public void Init(IMoveController moveController, float speed)
        {
            _moveController = moveController;
            _speed = speed;
        }

        public void SetTarget(ITarget target)
        {
            CurrentTarget = target;
        }

        private void Update()
        {
            if (CurrentTarget == null || !CurrentTarget.IsAlive) return;

            float distance = Vector2.Distance(transform.position, CurrentTarget.Transform.position);

            if (distance <= _attackRange)
            {
                Attack();
            }
            else
            {
                MoveTowardsTarget();
            }
        }

        private void MoveTowardsTarget()
        {
            Vector2 direction = (CurrentTarget.Transform.position - transform.position).normalized;
            _moveController?.Move(direction, _speed);
        }

        private void Attack()
        {
            _attackTimer += Time.deltaTime;
            if (_attackTimer < _attackCooldown) return;
            _attackTimer = 0;

            if (CurrentTarget is IDamageable damageable)
            {
                damageable.TakeDamage(_damage);
                Debug.Log($"{gameObject.name} attacked {CurrentTarget.Transform.name}");
            }
        }
    }
}
