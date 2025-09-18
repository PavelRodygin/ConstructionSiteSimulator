using System;
using CodeBase.Services.SceneInstallerService;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Test.PopupsTester.Scripts
{
    public class PopupsTesterSceneInstaller : SceneInstaller
    {
        [SerializeField] private PopupsTesterSceneView popupsTesterSceneView;
        [SerializeField] private TestButtonView buttonPrefab;

        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            builder.RegisterInstance(popupsTesterSceneView).As<PopupsTesterSceneView>();
            builder.Register<PopupsTesterScenePresenter>(Lifetime.Singleton)
                .As<IStartable>()
                .AsSelf();
            builder.Register<PopupsTesterSceneModel>(Lifetime.Singleton);
            
            builder.RegisterFactory<Action, TestButtonView>(action =>
            {
                var testButton = Instantiate(buttonPrefab, popupsTesterSceneView.buttonsParent);
                testButton.gameObject.SetActive(true);
                testButton.label.text = action.Method.Name;

                testButton.button.OnClickAsObservable()
                    .Subscribe(_ => action.Invoke())
                    .AddTo(testButton); 

                return testButton;
            });
        }

        public override void InjectSceneViews(IObjectResolver resolver) => 
            resolver.Inject(popupsTesterSceneView);
    }
}