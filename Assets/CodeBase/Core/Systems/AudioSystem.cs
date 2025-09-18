using System;
using System.Threading.Tasks;
using CodeBase.Core.Systems.Save;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace CodeBase.Core.Systems
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSystem : MonoBehaviour, ISerializableDataSystem
    {
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioClip mainMelodyClip;
        [SerializeField] private AudioClip gameMelodyClip;
        [Header("Fade Parameters")]
        [SerializeField] private float fadeDuration = 1.0f;
        [Inject] private SaveSystem _saveSystem;
        private float _musicVolume;

        public float MusicVolume
        {
            get => _musicVolume;
            private set => _musicVolume = value < 0 ? 0 : value;
        }

        public float SoundsVolume { get; private set; }

        public event Action<float> OnSoundsVolumeChanged;

        private void Start() => _saveSystem.AddSystem(this);

        public void PlayGameMelody() => PlayMusic(gameMelodyClip);
        
        public void PlayMainMenuMelody() => PlayMusic(mainMelodyClip);

        private void PlayMusic(AudioClip music)
        {
            musicAudioSource.clip = music;
            FadeIn(musicAudioSource, MusicVolume, fadeDuration);
        }
        
        public void StopMusic() => FadeOut(musicAudioSource, fadeDuration);

        public void SetMusicVolume(float volume)
        {
            Debug.Log("Set MusicVolume - " + volume);
            MusicVolume = volume > 0 ? volume : 0;
            musicAudioSource.volume = MusicVolume;
        }

        public void SetSoundsVolume(float volume)
        {
            SoundsVolume = volume > 0 ? volume : 0;
            OnSoundsVolumeChanged?.Invoke(SoundsVolume);
        }

        private void FadeOut(AudioSource audioSource, float duration)
        {
            audioSource.DOFade(0, duration).OnComplete(() => audioSource.Stop());
        }

        private void FadeIn(AudioSource audioSource, float targetVolume, float duration)
        {
            if (!audioSource.isPlaying) 
                audioSource.Play();
            
            audioSource.volume = 0;
            audioSource.DOFade(targetVolume, duration);
        }

        public UniTask LoadData(SerializableDataContainer dataContainer)
        {
            MusicVolume = dataContainer.TryGet(nameof(MusicVolume), out float musicVolume) ? musicVolume : 0;
            SoundsVolume = dataContainer.TryGet(nameof(SoundsVolume), out float soundsVolume) ? soundsVolume : 0;
            
            musicAudioSource.volume = MusicVolume;
            return UniTask.CompletedTask;
        }

        public void SaveData(SerializableDataContainer dataContainer)
        {      
            dataContainer.SetData(nameof(MusicVolume), MusicVolume);
            dataContainer.SetData(nameof(SoundsVolume), SoundsVolume);
        }
    }
}