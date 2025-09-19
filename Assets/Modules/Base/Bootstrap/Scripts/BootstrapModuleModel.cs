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
                "Always perform a pre-operation inspection on the crane to spot potential issues early!",
                "Wear your hard hat and high-visibility vest—PPE is your first line of defense on site.",
                "Keep a safe distance from overhead power lines to avoid electrical hazards.",
                "Never exceed the crane's rated load capacity; it's there for a reason!",
                "Use a qualified signal person for clear communication during lifts.",
                "Secure loads with proper rigging to prevent swings or drops mid-air.",
                "Monitor wind speeds—strong gusts can turn a stable lift into a wild ride.",
                "Follow lockout/tagout procedures before any maintenance work.",
                "Only certified operators should handle the controls; training saves lives.",
                "Report any crane defects immediately—safety first, always!"
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
