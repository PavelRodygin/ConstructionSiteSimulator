using CodeBase.Core.UI.Widgets.ProgressBars;
using TMPro;
using UnityEngine;

namespace CodeBase.Implementation.UI.Widgets.ProgressBars
{
    public class ObjectActivationProgressBar : BaseProgressBar
    {
        [SerializeField] private TMP_Text progressValueText;
        [SerializeField] private GameObject[] progressObjects;

        private int _currentActiveCount;
        
        protected override float GetCurrentProgress()
        {
            if (progressObjects == null || progressObjects.Length == 0)
                return 0f;

            return (float)_currentActiveCount / progressObjects.Length;
        }

        protected override void UpdateProgressVisual(float progress)
        {
            SetProgress(progress);
            if (progressValueText != null)
            {
                progressValueText.text = $"{(int)(progress * 100)}";
            }
        }

        public override void SetProgress(float progress)
        {
            int targetCount = Mathf.Clamp(Mathf.RoundToInt(progress * progressObjects.Length), 0, progressObjects.Length);
            SetActiveObjectCount(targetCount);
        }

        private void SetActiveObjectCount(int count)
        {
            _currentActiveCount = count;

            for (int i = 0; i < progressObjects.Length; i++)
            {
                progressObjects[i].SetActive(i < count);
            }
        }
    }
}