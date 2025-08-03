using Assets.Scripts.Enum;
using Assets.Scripts.Interfaces;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Character
{
    public class PlayerAnimationController : MonoBehaviour, IAnimator
    {
        public AnimationStates State { get; private set; }
        private Animator _animator;
        private readonly int isWalk = Animator.StringToHash("Walk");

        private IInput _input;

        [Inject]
        public void Construct(IInput input)
        {
            _input = input;
            _input.OnMove += OnMove;

            _animator = GetComponent<Animator>();
        }

        private void OnDestroy()
        {
            if (_input != null)
                _input.OnMove -= OnMove;
        }

        public void SetState(AnimationStates state)
        {
            switch (state)
            {
                case AnimationStates.Idle:
                    _animator.SetBool(isWalk, false);
                    break;
                case AnimationStates.Walk:
                    _animator.SetBool(isWalk, true);
                    break;
            }
        }

        public void SetTrigger(string state)
        {
            _animator.SetTrigger(state);
        }

        public void SetBool(string state, bool value)
        {
            _animator.SetBool(state, value);
        }

        private void OnMove(Vector2 move)
        {
            SetState(move == Vector2.zero ? AnimationStates.Idle : AnimationStates.Walk);
        }
    }
}

