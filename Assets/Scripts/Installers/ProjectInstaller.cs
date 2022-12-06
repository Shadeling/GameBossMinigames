using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MyGame
{

    public class ProjectInstaller : MonoInstaller<ProjectInstaller>
    {
        [SerializeField] StateHolder stateHolder;
        public override void InstallBindings()
        {
            Bind<StateHolder>(stateHolder, "StateHolder");

            Container.Bind<UIFields>().FromComponentInNewPrefabResource("UI/UIRoot").AsSingle().NonLazy();
            Container.Bind<IInventory>().To<Inventory>().AsSingle().NonLazy();

        }


        void Bind<TContract>(UnityEngine.Object prefab, string resourceName, bool forceResource = false, bool nonLazy = false)
        {
            string resourcePath = "_ProjectContext/" + resourceName;
            ConcreteIdBinderGeneric<TContract> binderGeneric = Container.Bind<TContract>();
            NameTransformScopeConcreteIdArgConditionCopyNonLazyBinder binder;
            if (forceResource)
                binder = binderGeneric.FromComponentInNewPrefabResource(resourcePath);
            else
            {
                UnityEngine.Object resource = Resources.Load(resourcePath);
                binder = binderGeneric.FromComponentInNewPrefab(resource != null ? resource : prefab);
            }

            binder.AsSingle();

            if (nonLazy)
                binder.NonLazy();
        }

    }
}
