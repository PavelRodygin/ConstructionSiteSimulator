using System;
using UnityEngine.UI;

namespace CodeBase.Core.UI.Widgets.ProgressBars
{
    public class LegacyProgressBarView : BaseLegacyProgressBarView
    {
        public Image image;
        public override void Report(float value)
        {
            if (image != null)
                image.fillAmount = Math.Min(value, 1f); //Ensures that value does not exceed 1 (the maximum fill amount)
        }
        public override void ReportToZero(float value)
        {
            if (image != null)
                image.fillAmount = Math.Max(value, 0f);
        }
    }
}