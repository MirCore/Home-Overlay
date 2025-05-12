using System;
using System.Collections;
using UnityEngine;
using Utils;

namespace Managers
{
    public class SoundManager : Singleton<SoundManager>
    {
        private static AudioSource _audioSource;
        private static DateTime _lastPlayed;
        
        [SerializeField] private AudioClip ClickSound;
        [SerializeField] private AudioClip DeleteSound;
        private AudioConfiguration configAudio;

        private void OnEnable()
        {
            configAudio = AudioSettings.GetConfiguration();
            _audioSource = GetComponent<AudioSource>();
        }

        public static void OnUIPressed()
        {
            TimeSpan timeSinceLastPlayed = DateTime.Now - _lastPlayed;
            if (_audioSource.isPlaying || timeSinceLastPlayed.TotalSeconds < 0.2) 
                return;
            _audioSource.Play();
            _lastPlayed = DateTime.Now;
        }

        public static void OnUIClicked()
        {
            TimeSpan timeSinceLastPlayed = DateTime.Now - _lastPlayed;
            if (_audioSource.isPlaying || timeSinceLastPlayed.TotalSeconds < 0.2) 
                return;
            _audioSource.PlayOneShot(Instance.ClickSound);
            _lastPlayed = DateTime.Now;
        }

        public static void OnUIDeleted()
        {
            TimeSpan timeSinceLastPlayed = DateTime.Now - _lastPlayed;
            if (_audioSource.isPlaying || timeSinceLastPlayed.TotalSeconds < 0.2) 
                return;
            _audioSource.PlayOneShot(Instance.DeleteSound);
            _lastPlayed = DateTime.Now;
        }
        
        public void ResetAudio()
        {
            StartCoroutine(ResetAudioAfterClosingNativeWindow());
        }
        
        private static IEnumerator ResetAudioAfterClosingNativeWindow()
        { 
            yield return new WaitForSeconds(0.25f);
            AudioSettings.Reset(Instance.configAudio);
            //Play your track again after this.
        }
    }
}