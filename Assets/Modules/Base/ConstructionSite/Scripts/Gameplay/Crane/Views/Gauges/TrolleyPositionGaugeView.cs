using System.Globalization;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Base.ConstructionSite.Scripts.Gameplay.Crane.Views
{
    public class TrolleyPositionGaugeView : MonoBehaviour
    {
        [SerializeField] private TMP_Text positionValueText;
        [SerializeField] private Image forwardIndicatorImage;
        [SerializeField] private Image backwardIndicatorImage;
        [SerializeField] private Trolley trolley;
        
        private float _previousPosition;
        
        private void Start()
        {
            if (!trolley)
            {
                Debug.LogError("Trolley reference is null in TrolleyPositionGaugeView");
                return;
            }
            
            SetupReactiveSubscriptions();
        }
        
        private void SetupReactiveSubscriptions()
        {
            trolley.RelativeZPosition
                .Where(_ => gameObject.activeInHierarchy)
                .Subscribe(OnPositionChanged)
                .AddTo(this);
        }
        
        private void OnPositionChanged(float trolleyPosition)
        {
            positionValueText.text = trolleyPosition.ToString("F1", CultureInfo.InvariantCulture);
            UpdateMovementIndicators(trolleyPosition);
            _previousPosition = trolleyPosition;
        }
        
        private void UpdateMovementIndicators(float currentPosition)
        {
            bool isMovingForward = currentPosition > _previousPosition;
            bool isMovingBackward = currentPosition < _previousPosition;
            bool isStopped = !isMovingForward && !isMovingBackward;
            
            // Show both indicators when stopped, individual indicators when moving
            UpdateForwardIndicator(isMovingForward || isStopped);
            UpdateBackwardIndicator(isMovingBackward || isStopped);
        }
        
        private void UpdateForwardIndicator(bool isMoving)
        {
            if (forwardIndicatorImage) 
                forwardIndicatorImage.gameObject.SetActive(isMoving);
        }
        
        private void UpdateBackwardIndicator(bool isMoving)
        {
            if (backwardIndicatorImage) 
                backwardIndicatorImage.gameObject.SetActive(isMoving);
        }
    }
}