using Assets.Scripts.Interfaces;
using Assets.Scripts.UI;
using UnityEngine;
using Zenject;

namespace Assets.Installer
{
    public class GameUIInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ICurrencyView>().FromComponentInHierarchy().AsTransient();
            Container.Bind<CurrencyUiController>().AsSingle().NonLazy();
        }
    }
}
