using System.Collections.Generic;
using CodeBase.Core.UI.Views;
using R3;
using UnityEngine;

namespace Modules.Test.PopupsTester.Scripts
{
    public class PopupsTesterSceneView : BaseView
    {
        [SerializeField] public Transform buttonsParent;
        private List<TestButtonView> _testButtonViews;

        private new void Awake()
        {
            base.Awake();
            HideInstantly();
        }

        public void GetPopupsButtons(List<TestButtonView> testButtons) => 
            _testButtonViews = testButtons;

        public void SetupListeners(Dictionary<TestButtonView, ReactiveCommand<Unit>> buttonCommandMap)
        {
            foreach (var button in buttonCommandMap.Keys)
            {
                button.OnClickAsObservable()
                    .Subscribe(_ => buttonCommandMap[button].Execute(default))
                    .AddTo(this);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            RemoveEventListeners();
        }

        private void RemoveEventListeners()
        {
            foreach (var testButton in _testButtonViews)
                testButton.button.onClick.RemoveAllListeners();
        }
    }
}