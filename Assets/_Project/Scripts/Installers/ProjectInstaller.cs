using Assets.Scripts.Input;
using Zenject;

namespace Assets.Installer
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindInput();
        }

        private void BindInput()
        {
#if UNITY_EDITOR
            Container.BindInterfacesAndSelfTo<KeyboardInputController>().AsSingle();
#elif UNITY_ANDROID
    // Touch 
#endif
        }
    }
}
