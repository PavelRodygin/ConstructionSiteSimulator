using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace CodeBase.Core.UI.Views.Animations
{
    public abstract class BaseAnimationElement : MonoBehaviour, IAnimationElement
    {
        protected Sequence Sequence;
        
        public abstract UniTask Show();
        public abstract UniTask Hide();
        
        private void OnDisable() => Sequence.Kill();
    }
}