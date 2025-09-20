using CodeBase.Core.UI.Views;
using UnityEngine;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane.Views
{
    public partial class CraneDashboardView : BaseView
    {
        [SerializeField] private TurntableGaugeView turntableGauge;
        [SerializeField] private TrolleyPositionGaugeView trolleyPositionGauge;
        [SerializeField] private HookLoadGaugeView hookLoadGauge;
    }
}