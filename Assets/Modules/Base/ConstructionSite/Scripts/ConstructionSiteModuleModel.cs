using CodeBase.Core.Patterns.Architecture.MVP;

namespace Modules.Base.ThirdPersonMPModule.Scripts
{
    /// <summary>
    /// Model for ThirdPersonMP module that contains business logic and data
    /// </summary>
    public class ConstructionSiteModuleModel : IModel
    {
        /// <summary>
        /// Delay for command throttling to prevent rapid interactions
        /// </summary>
        public int CommandThrottleDelay => 300;
        
        /// <summary>
        /// Delay for module transition throttling to prevent rapid module switching
        /// </summary>
        public int ModuleTransitionThrottleDelay => 500;

        public void Dispose() { }
    }
}
