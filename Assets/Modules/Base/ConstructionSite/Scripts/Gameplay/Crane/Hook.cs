using Modules.Base.ThirdPersonMPModule.Scripts.Gameplay;
using UnityEngine;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane
{
    public class Hook : MonoBehaviour
    {
        private Cargo _cargo;
        
        [field: SerializeField] public ConfigurableJoint Joint {get; private set;}
        
        //Отвечает за прицепку/отцепку грузов. Как я понял, должен динамически создавать Joint для сцепки себя с грузом. Также, должен определять груз с помощью метода OnTriggerEnter
        //Надо понять. Что будет эффективнее, разместить на каждом грузе по TriggerCollider или только на крюке, чтобы тот сканировал близко ли AttachmentPoint. Думаю, что лучше на грузах TriggerCollider-ы
    }
}