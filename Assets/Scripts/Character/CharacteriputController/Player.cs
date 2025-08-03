using Assets.ScriptableObjects;
using Assets.Scripts.Interfaces;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Character
{
    public class Player : MonoBehaviour
    {
        public IMoveController MoveController { get; private set; }
        public IAnimator Animator { get; private set; }
        public float Speed { get; private set; }

        [Inject]
        public void Init(PlayerConfig config)
        {
            Speed = config.Speed;
            MoveController = GetComponent<IMoveController>();
            Animator = GetComponent<IAnimator>();
        }   


    }
}
