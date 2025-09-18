using Cysharp.Threading.Tasks;

namespace CodeBase.Core.UI.Views.Animations
{
    public interface IAnimationElement
    {
        UniTask Show();
        UniTask Hide();
    }
}