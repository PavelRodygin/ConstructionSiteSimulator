using System;
using Cysharp.Threading.Tasks;

namespace CodeBase.Core.UI.Views
{
    public interface IView : IDisposable
    {
        public UniTask Show();

        public UniTask Hide();

        public void HideInstantly();
    }
}