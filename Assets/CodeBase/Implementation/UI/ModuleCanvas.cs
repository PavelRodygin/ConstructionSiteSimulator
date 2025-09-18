using CodeBase.Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.Implementation.UI
{
    /// <summary>
    /// Implementation of ModuleCanvas inheriting from BaseModuleCanvas.
    /// </summary>
    public class ModuleCanvas : BaseModuleCanvas
    {
        /// <summary>
        /// Specific initialization logic for ModuleCanvas.
        /// </summary>
        public override void InitializeCanvas()
        {
            // Add specific initialization logic here if needed.
            Debug.Log("ModuleCanvas initialized.");
        }
    }
}