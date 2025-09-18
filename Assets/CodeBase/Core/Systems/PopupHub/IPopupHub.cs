using Cysharp.Threading.Tasks;

namespace CodeBase.Core.Systems.PopupHub
{
    public interface IPopupHub
    {
        UniTask TryOpenNextPopup();
        UniTask TryOpenNextPopup<T>(T param);
        UniTask CloseCurrentPopup();
        void NotifyPopupClosed();
        void OpenFirstPopup();
        void OpenSecondPopup();
        void OpenThirdPopup();
        void OpenSettingsPopup();
    }
}