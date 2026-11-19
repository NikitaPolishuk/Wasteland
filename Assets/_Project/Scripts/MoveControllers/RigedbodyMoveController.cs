using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.MoveControllers
{
    public class RigedbodyMoveController : MonoBehaviour, IMoveController
    {
        private Rigidbody2D _rb;
        private Collider2D _collider;
        private float _currentSpeed;
        private Vector2 _direction;
        private float _leftBound = float.NegativeInfinity;
        private float _rightBound = float.PositiveInfinity;

        public void ConfigureBounds(float leftBound, float rightBound)
        {
            _leftBound = leftBound;
            _rightBound = rightBound;
        }

        public void Move(Vector2 direction, float speed)
        {
            _currentSpeed = speed;
            _direction = new Vector2(direction.x == 0 ? 0 : Mathf.Sign(direction.x), 0);
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
        }

        private void FixedUpdate()
        {
            float velocity = _direction.x * _currentSpeed;
            if (_collider != null)
            {
                float leftDistance = Mathf.Max(0, _collider.bounds.min.x - _leftBound);
                float rightDistance = Mathf.Max(0, _rightBound - _collider.bounds.max.x);
                velocity = Mathf.Clamp(velocity, -leftDistance / Time.fixedDeltaTime, rightDistance / Time.fixedDeltaTime);
            }

            _rb.linearVelocity = new Vector2(velocity, _rb.linearVelocity.y);
        }
    }
}

