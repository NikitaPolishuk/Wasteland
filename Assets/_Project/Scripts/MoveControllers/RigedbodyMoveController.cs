using System;
using Assets.ScriptableObjects;
using Assets.Scripts.Interfaces;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.MoveControllers
{
    public class RigedbodyMoveController : MonoBehaviour, IMoveController
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rb;

        private float _currentSpeed = 0;
        private Vector2 _direction;
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void FixedUpdate()   
        {
            _rb.linearVelocity = new Vector2(_direction.x * _currentSpeed, _rb.linearVelocity.y);
            if (_direction.x < 0 && !_spriteRenderer.flipX) _spriteRenderer.flipX = true;
            else if (_direction.x > 0 && _spriteRenderer.flipX)
            {
                _spriteRenderer.flipX = false;
            }
            
            Debug.Log(_direction);
        }

        public void Move(Vector2 direction, float speed)
        {
            _currentSpeed = speed;
            _direction = direction;
        }
    }
}
