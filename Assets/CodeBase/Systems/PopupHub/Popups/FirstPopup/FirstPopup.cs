using CodeBase.Core.Systems.PopupHub;
using CodeBase.Core.Systems.PopupHub.Popups;

namespace CodeBase.Systems.PopupHub.Popups.FirstPopup
{
    public class FirstPopup : BasePopup
    {
        protected override void Awake()
        {
            priority = PopupsPriority.High; 
            base.Awake();
        }
    }
}