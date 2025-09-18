using CodeBase.Core.UI.Views.Animations;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Test.PopupsTester.Scripts
{
    public class TestButtonView : MonoBehaviour
    {
        [SerializeField] private BaseAnimationElement animationElement;
        public Button button;
        public TMP_Text label;

        public virtual async UniTask Show()
        {
            gameObject.SetActive(true);
            if (animationElement != null)
                await animationElement.Show();
        }

        public virtual async UniTask Hide()
        {
            if (animationElement != null) await animationElement.Hide();
            gameObject.SetActive(false);
        }

        public void HideInstantly() => gameObject.SetActive(false);

        public Observable<Unit> OnClickAsObservable() => button.onClick.AsObservable();
    }
}