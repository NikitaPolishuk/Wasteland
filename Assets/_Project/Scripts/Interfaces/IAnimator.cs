using Assets.Scripts.Enum;

namespace Assets.Scripts.Interfaces
{
    public interface IAnimator
    {
        AnimationStates State { get; }
        void SetState(AnimationStates state);
    }
}
