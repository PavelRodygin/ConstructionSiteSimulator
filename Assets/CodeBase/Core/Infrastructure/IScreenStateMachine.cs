using CodeBase.Core.Infrastructure.Modules;
using Cysharp.Threading.Tasks;

namespace CodeBase.Core.Infrastructure
{
    public interface  IScreenStateMachine
    {
        public IModuleController CurrentModuleController { get; }
        
        UniTaskVoid RunModule(ModulesMap modulesMap, bool splashScreenRequired = false,
            object param = null);
    }
    
    public static class ScreenStateMachineExtension
    {
        public static UniTaskVoid RunModule(this IScreenStateMachine self, bool splashScreenRequired,
            ModulesMap modulesMap) 
            => self.RunModule(modulesMap);
    }
}