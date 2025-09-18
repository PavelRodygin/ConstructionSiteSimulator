using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.Core.UI
{
    /// <summary>
    /// Base abstract class for managing UI canvas functionality.
    /// Provides common methods and properties for UI canvas-related operations.
    /// </summary>
    public abstract class BaseModuleCanvas : MonoBehaviour
    {
        [SerializeField] protected CanvasScaler canvasScaler;
        [SerializeField] protected Camera uiCamera;

        /// <summary>
        /// The camera used for rendering the UI.
        /// </summary>
        public Camera UICamera => uiCamera;

        private float? _scaleFactor;

        /// <summary>
        /// Calculates and returns the scaling factor for the UI canvas based on the screen resolution and 
        /// the settings of the associated <see cref="CanvasScaler"/>.
        /// This factor determines how UI elements should scale to match the screen size.
        /// </summary>
        /// <returns>
        /// A float representing the scale factor. If the canvas is set to 
        /// <see cref="CanvasScaler.ScreenMatchMode.MatchWidthOrHeight"/>, the scale factor is computed
        /// based on the weighted average of the screen width and height relative to the reference resolution.
        /// Otherwise, a default scale factor of 1 is returned.
        /// </returns>
        public float GetScaleFactor()
        {
            if (_scaleFactor.HasValue) return _scaleFactor.Value;
            _scaleFactor = 1f;

            if (canvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
            {
                var logWidth = Mathf.Log(Screen.width / canvasScaler.referenceResolution.x, 2);
                var logHeight = Mathf.Log(Screen.height / canvasScaler.referenceResolution.y, 2);
                var logWeightedAverage = Mathf.Lerp(logWidth, logHeight, canvasScaler.matchWidthOrHeight);
                _scaleFactor = Mathf.Pow(2, logWeightedAverage);
            }

            return _scaleFactor.Value;
        }

        /// <summary>
        /// Calculates and returns the screen-space bounding rectangle of a given <see cref="RectTransform"/>.
        /// The resulting rectangle is defined in pixel coordinates relative to the screen.
        /// </summary>
        /// <param name="rectTransform">The <see cref="RectTransform"/> whose screen-space bounds are to be calculated.</param>
        /// <returns>
        /// A <see cref="Rect"/> representing the bounding rectangle in screen space, where:
        /// - The bottom-left corner is given by the rectangle's (x, y) position.
        /// - The width and height are measured in screen pixels.
        /// </returns>
        public Rect GetScreenSpaceBounds(RectTransform rectTransform)
        {
            var worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);

            var screenBottomLeft = UICamera.WorldToScreenPoint(worldCorners[0]);
            var screenTopRight = UICamera.WorldToScreenPoint(worldCorners[2]);

            var x = screenBottomLeft.x;
            var y = screenBottomLeft.y;
            var width = screenTopRight.x - screenBottomLeft.x;
            var height = screenTopRight.y - screenBottomLeft.y;

            return new Rect(x, y, width, height);
        }

        /// <summary>
        /// Abstract method for initialization logic specific to derived canvas classes.
        /// </summary>
        public abstract void InitializeCanvas();
    }
}
