using CodeBase.Core.Systems.PopupHub;
using CodeBase.Core.Systems.PopupHub.Popups;

namespace CodeBase.Systems.PopupHub.Popups.ThirdPopup
{
    public class ThirdPopup : BasePopup
    {
        protected override void Awake()
        {
            priority = PopupsPriority.Low; 
            base.Awake();
        }
    }
}