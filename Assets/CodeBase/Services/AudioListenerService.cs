using UnityEngine;
using VContainer;

namespace CodeBase.Services
{
    public class AudioListenerService
    {
        private const string MainCameraName = "MainCamera";
        
        public void EnsureAudioListenerExists(IObjectResolver resolver)
        {
            var mainCamera = resolver.Resolve<Camera>();
            if (mainCamera == null)
            {
                mainCamera = new GameObject(MainCameraName).AddComponent<Camera>();
                resolver.Inject(mainCamera);
            }

            var audioListener = mainCamera.GetComponent<AudioListener>();
            if (audioListener == null)
                mainCamera.gameObject.AddComponent<AudioListener>();
        }
    }
}