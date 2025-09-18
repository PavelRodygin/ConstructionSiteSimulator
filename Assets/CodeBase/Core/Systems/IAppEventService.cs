using System;

namespace CodeBase.Core.Systems
{
    public interface IAppEventService
    {
        public event Action<bool> OnApplicationFocusEvent;
        public event Action<bool> OnApplicationPauseEvent;
        public event Action OnApplicationQuitEvent;
    }
}