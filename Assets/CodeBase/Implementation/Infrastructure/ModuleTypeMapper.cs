using System;
using System.Collections.Generic;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using Modules.Base.Bootstrap.Scripts;
using Modules.Base.MainMenu.Scripts;
using Modules.Base.ThirdPersonMPModule.Scripts;
using VContainer;

namespace CodeBase.Implementation.Infrastructure
{
    ///<summary> 
    /// Responsible for resolving (or instantiating) the appropriate module controller for the
    /// specified ModulesMap, using a dependency injection container provided by sceneLifetimeScope
    ///</summary>
    public class ModuleTypeMapper
    {
        private readonly Dictionary<ModulesMap, Type> _map;

        public ModuleTypeMapper()
        {
            _map = new Dictionary<ModulesMap, Type> 
            {
                { ModulesMap.Bootstrap, typeof(BootstrapModuleController) },
                { ModulesMap.MainMenu, typeof(MainMenuModuleController) },
                { ModulesMap.ConstructionSite, typeof(ConstructionSiteModuleController) },
            };
        }

        public IModuleController Resolve(ModulesMap modulesMap, IObjectResolver objectResolver) => 
            (IModuleController)objectResolver.Resolve(_map[modulesMap]);
        
        public IModuleController ResolveModuleController(ModulesMap modulesMap, IObjectResolver objectResolver) =>
        (IModuleController)objectResolver.Resolve(_map[modulesMap]);
    }
}