using CodeBase.Core.Patterns.ObjectCreation;
using UnityEngine;
using VContainer;

namespace CodeBase.Core.Systems.PopupHub.Popups
{
    public interface IBasePopupFactory<out T> : IFactory<Transform, T> where T : BasePopup { }

    public class BasePopupFactory<T> : IBasePopupFactory<T> where T : BasePopup
    {
        private readonly IObjectResolver _resolver;
        private readonly T _basePopupFactoryPrefab;

        public BasePopupFactory(IObjectResolver resolver, T basePopupFactoryPrefab)
        {
            _resolver = resolver;
            _basePopupFactoryPrefab = basePopupFactoryPrefab;
        }

        public virtual T Create(Transform parentTransform)
        {
            var popupInstance = Object.Instantiate(_basePopupFactoryPrefab, parentTransform);
            _resolver.Inject(popupInstance);  // injects all the dependencies into the popup
            popupInstance.gameObject.SetActive(false);
            return popupInstance;
        }
    }
}