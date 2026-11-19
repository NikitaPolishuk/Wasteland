namespace Assets.Scripts.Interfaces
{
    public interface IAttacker
    {
        void SetTarget(ITarget target);
        ITarget CurrentTarget { get; }
    }
}
