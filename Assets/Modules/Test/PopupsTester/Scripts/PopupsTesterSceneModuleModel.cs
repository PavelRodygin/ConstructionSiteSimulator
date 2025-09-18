using System;
using CodeBase.Core.Patterns.Architecture.MVP;
using CodeBase.Core.Systems.PopupHub;
using VContainer;

namespace Modules.Test.PopupsTester.Scripts
{
    public class PopupsTesterSceneModel : IModel
    {
        private readonly Func<Action, TestButtonView> _buttonFactory;
        private readonly Action[] _popupActions;

        [Inject] public PopupsTesterSceneModel(IPopupHub popupHub)
        {
            var popupHub1 = popupHub;

            _popupActions = new Action[]
            {
                popupHub1.OpenFirstPopup,
                popupHub1.OpenSecondPopup,
                popupHub1.OpenThirdPopup
            };
        }

        public Action[] GetPopupHubActions() => _popupActions;

        public void Dispose() { }
    }
}