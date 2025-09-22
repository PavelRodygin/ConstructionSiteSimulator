using System.Globalization;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane.Views
{
    public class HookHeightGaugeView : MonoBehaviour
    {
        [SerializeField] private Hook hook;
        [SerializeField] private TMP_Text heightValueText;
        [SerializeField] private Image upIndicator;
        [SerializeField] private Image downIndicator;
        
        private void Start()
        {
            if (!hook)
            {
                Debug.LogError("Hook reference is null in HookHeightGaugeView");
                return;
            }
            
            if (!heightValueText)
            {
                Debug.LogError("HeightValueText reference is null in HookHeightGaugeView");
                return;
            }
            
            if (!upIndicator)
            {
                Debug.LogError("UpIndicator reference is null in HookHeightGaugeView");
                return;
            }
            
            if (!downIndicator)
            {
                Debug.LogError("DownIndicator reference is null in HookHeightGaugeView");
                return;
            }
            
            SetupReactiveSubscriptions();
        }
        
        private void SetupReactiveSubscriptions()
        {
            hook.CurrentHeight
                .Where(_ => gameObject.activeInHierarchy)
                .Subscribe(OnHeightChanged)
                .AddTo(this);
            
            hook.VerticalSpeed
                .Where(_ => gameObject.activeInHierarchy)
                .Subscribe(OnSpeedChanged)
                .AddTo(this);
        }
        
        private void OnHeightChanged(float currentHeight)
        {
            heightValueText.text = currentHeight.ToString("F1", CultureInfo.InvariantCulture) + " m";
        }
        
        private void OnSpeedChanged(float currentSpeed)
        {
            upIndicator.gameObject.SetActive(currentSpeed > 0.1f);
            downIndicator.gameObject.SetActive(currentSpeed < -0.1f);
        }
    }
}