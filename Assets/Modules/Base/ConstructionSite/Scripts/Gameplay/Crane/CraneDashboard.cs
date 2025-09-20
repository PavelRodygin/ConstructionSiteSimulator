using Modules.Base.ConstructionSite.Scripts.Gameplay.Crane.Views;
using UnityEngine;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane
{
    public class CraneDashboard : MonoBehaviour
    {
        [SerializeField] private Canvas worldSpaceCanvas;
        
        [field: SerializeField] public CraneDashboardView CraneDashboardView { get; private set; }
    }
}