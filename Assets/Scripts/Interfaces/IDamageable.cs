using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(int amount);
        bool IsAlive { get; }
        int CurrentHp { get; }
        int MaxHp { get; }
    }
}
