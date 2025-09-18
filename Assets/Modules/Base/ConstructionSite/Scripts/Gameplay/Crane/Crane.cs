using CodeBase.Services.Input;
using UnityEngine;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane
{
    public class Crane : MonoBehaviour
    {
        [SerializeField] private Trolley trolley;
        [SerializeField] private Transform rotatingBase;
        
        private InputSystemService _inputSystemService;
        
        
        private void Construct(InputSystemService inputSystemService)
        {
            _inputSystemService = inputSystemService;
        }
    }
}
