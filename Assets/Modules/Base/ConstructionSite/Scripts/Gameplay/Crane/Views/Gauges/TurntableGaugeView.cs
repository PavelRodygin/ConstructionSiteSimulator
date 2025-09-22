using System.Globalization;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane.Views
{
    public class TurntableGaugeView : MonoBehaviour
    {
        [SerializeField] private TMP_Text speedValueText;
        [SerializeField] private Image rotationImage;
        [SerializeField] private Turntable turntable;

        private void Start()
        {
            if (!turntable)
            {
                Debug.LogError("Turntable reference is null in TurntableGaugeView");
                return;
            }
            
            if (!speedValueText)
            {
                Debug.LogError("SpeedValueText reference is null in TurntableGaugeView");
                return;
            }
            
            if (!rotationImage)
            {
                Debug.LogError("RotationImage reference is null in TurntableGaugeView");
                return;
            }
            
            SetupReactiveSubscriptions();
        }
        
        private void SetupReactiveSubscriptions()
        {
            turntable.CurrentRotationSpeed
                .Where(_ => gameObject.activeInHierarchy)
                .Subscribe(OnSpeedChanged)
                .AddTo(this);
            
            turntable.CurrentRotationAngle
                .Where(_ => gameObject.activeInHierarchy)
                .Subscribe(OnAngleChanged)
                .AddTo(this);
        }
        
        private void OnSpeedChanged(float currentSpeed)
        {
            speedValueText.text = Mathf.Abs(currentSpeed).ToString("F1", CultureInfo.InvariantCulture);
        }
        
        private void OnAngleChanged(float currentAngle)
        {
            rotationImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -currentAngle);
        }
    }
}