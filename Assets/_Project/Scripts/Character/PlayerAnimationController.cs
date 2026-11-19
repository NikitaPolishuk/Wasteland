using Assets.Scripts.Enum;
using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Character
{
    public class PlayerAnimationController : MonoBehaviour, IAnimator
    {
        private const float MOVEMENT_EPSILON = 0.0001f;
        private readonly int _isWalk = Animator.StringToHash("Walk");
        private Animator _animator;
        private Rigidbody2D _body;
        private SpriteRenderer _spriteRenderer;
        private float _previousX;

        public AnimationStates State { get; private set; }

        public void SetState(AnimationStates state)
        {
            State = state;
            _animator.SetBool(_isWalk, state == AnimationStates.Walk);
        }

        public void SetTrigger(string state)
        {
            _animator.SetTrigger(state);
        }

        public void SetBool(string state, bool value)
        {
            _animator.SetBool(state, value);
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _body = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _previousX = _body.position.x;
        }

        private void FixedUpdate()
        {
            float currentX = _body.position.x;
            bool isMoving = Mathf.Abs(currentX - _previousX) > MOVEMENT_EPSILON;
            if (isMoving && _spriteRenderer != null)
            {
                _spriteRenderer.flipX = currentX < _previousX;
            }

            SetState(isMoving ? AnimationStates.Walk : AnimationStates.Idle);
            _previousX = currentX;
        }
    }
}

