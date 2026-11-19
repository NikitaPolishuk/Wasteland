using Assets.Scripts.Interfaces;
using Assets.Scripts.Wallet;
using Zenject;

namespace Assets.Scripts.Character
{
    public class InteractableService : IInteractableService
    {
        public IInteractable Current { get; private set; }
        private IInput _inputReader;

        [Inject]
        InteractableService(IInput input)
        {
            _inputReader = input;
            _inputReader.OnIntection += Interact;
        }

        public void Set(IInteractable interactable)
        {
            Current = interactable;
        }

        public void Clear(IInteractable interactable)
        {
            if (Current == interactable) Current = null;
        }

        public void Interact()
        {
            Current?.Interactable();
        }
    }
}
