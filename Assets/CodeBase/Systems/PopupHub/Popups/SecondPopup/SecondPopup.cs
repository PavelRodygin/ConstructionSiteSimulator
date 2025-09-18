using CodeBase.Core.Systems.PopupHub;
using CodeBase.Core.Systems.PopupHub.Popups;

namespace CodeBase.Systems.PopupHub.Popups.SecondPopup
{
    public class SecondPopup : BasePopup
    {
        protected override void Awake()
        {
            priority = PopupsPriority.Medium; 
            base.Awake();
        }
    }
}