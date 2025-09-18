using CodeBase.Core.Patterns.Architecture.MVP;

namespace Modules.Base.MainMenu.Scripts
{
    public class MainMenuModuleModel : IModel
    {
        // Throttle delays for anti-spam protection
        public int CommandThrottleDelay => 300;
        public int ModuleTransitionThrottleDelay => 500;

        public MainMenuModuleModel() { }

        public void Dispose() { }
    }
}