using UnityEngine;

namespace CodeBase.Core.UI
{
    public abstract class BasePopupCanvas : BaseModuleCanvas
    {
        [SerializeField] private Transform popupParent;
        
        public Transform PopupParent => popupParent;    
        
        public override void InitializeCanvas()
        {
            // Add specific initialization logic here if needed.
            Debug.Log("PopupCanvas initialized.");
        }
    }
}