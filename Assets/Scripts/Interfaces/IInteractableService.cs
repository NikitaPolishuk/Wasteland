namespace Assets.Scripts.Interfaces
{
    public interface IInteractableService
    {
        IInteractable Current { get;}
        void Set(IInteractable interactable);
        void Clear(IInteractable interactable);
        void Interact();
    }
}
