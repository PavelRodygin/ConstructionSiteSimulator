using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Core.Patterns.Architecture.MVP;
using CodeBase.Services.LongInitializationServices;
using DG.Tweening;
using DG.Tweening.Core.Enums;

namespace Modules.Base.Bootstrap.Scripts
{
    public class BootstrapModuleModel : IModel
    {
        public int ModuleTransitionThrottleDelay => 1000;
        public int TooltipDelay => 3000;
        public int AppFrameRate => 60;
        
        private readonly FirstLongInitializationService _firstLongInitializationService;
        private readonly SecondLongInitializationService _secondLongInitializationService;
        private readonly ThirdLongInitializationService _thirdLongInitializationService;
        public readonly Dictionary<string, Func<Task>> Commands;
        private readonly string[] _tooltips;
        
        private int _currentTooltipIndex;

        public BootstrapModuleModel(FirstLongInitializationService firstLongInitializationService,
            SecondLongInitializationService secondLongInitializationService,
            ThirdLongInitializationService thirdLongInitializationService)
        {
            _firstLongInitializationService = firstLongInitializationService;
            _secondLongInitializationService = secondLongInitializationService;
            _thirdLongInitializationService = thirdLongInitializationService;

            Commands = new Dictionary<string, Func<Task>>();
            
            _tooltips = new []
            {
                "Shrimp can change color to blend into their surroundings!",
                "A shrimp's heart is located in its head.",
                "Some shrimp species can live up to 6 years in the wild.",
                "Shrimp have 10 legs, but only the front two are used for walking.",
                "The smallest shrimp species is only about 1 cm long.",
                "Shrimp can swim backward by quickly flexing their tails.",
                "Certain shrimp can generate a sound louder than a gunshot with their claws!",
                "Shrimp have a lifespan that varies from a few months to several years.",
                "Some shrimp species are known to clean parasites off other sea creatures.",
                "Shrimp are a key part of the ocean food chain, feeding many larger animals."
            };
        }
        
        public void DoTweenInit()
        {
            DOTween.Init().SetCapacity(240, 30);
            DOTween.safeModeLogBehaviour = SafeModeLogBehaviour.None;
            DOTween.defaultAutoKill = true;
            DOTween.defaultRecyclable = true;
            DOTween.useSmoothDeltaTime = true;
        }

        public void RegisterCommands()
        {
            Commands.Add("First Service", _firstLongInitializationService.Init);
            Commands.Add("Second Service", _secondLongInitializationService.Init);
            Commands.Add("Third Service", _thirdLongInitializationService.Init);
        }
        
        public string GetNextTooltip()
        {
            var tooltip = _tooltips[_currentTooltipIndex];
            _currentTooltipIndex = (_currentTooltipIndex + 1) % _tooltips.Length;
            return tooltip;
        }

        public void Dispose() => Commands.Clear();
    }
}
