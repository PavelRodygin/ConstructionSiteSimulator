using System;
using CodeBase.Core.Systems;
using UnityEngine;

namespace CodeBase.Systems
{
	public class UniversalAppEventsService: MonoBehaviour, IAppEventService
	{
		public event Action<bool> OnApplicationFocusEvent;
		public event Action<bool> OnApplicationPauseEvent;
		public event Action OnApplicationQuitEvent;
		
		private void OnApplicationFocus(bool focus)
		{
			OnApplicationFocusEvent?.Invoke(focus);
		}

		private void OnApplicationPause(bool pause)
		{
			OnApplicationPauseEvent?.Invoke(pause);
		}

		private void OnApplicationQuit()
		{
			OnApplicationQuitEvent?.Invoke();
		}
	}
}

/* Тестовая версия без

using System;
using UnityEngine;

namespace Scripts.Services.AppEvents
{
    public class UniversalAppEventsService : IAppEventService
    {
        public event Action<bool> OnApplicationFocusEvent;
        public event Action<bool> OnApplicationPauseEvent;
        public event Action OnApplicationQuitEvent;

        public UniversalAppEventsService()
        {
            Application.focusChanged += OnApplicationFocus;
            Application.wantsToQuit += OnApplicationQuit;
        }

        private void OnApplicationFocus(bool focus)
        {
            OnApplicationFocusEvent?.Invoke(focus);
        }

        private void OnApplicationQuit()
        {
            OnApplicationQuitEvent?.Invoke();
        }

        public void OnApplicationPause(bool pause)
        {
            OnApplicationPauseEvent?.Invoke(pause);
        }

        public void Dispose()
        {
            Application.focusChanged -= OnApplicationFocus;
            Application.wantsToQuit -= OnApplicationQuit;
        }
    }
}

*/