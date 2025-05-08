using UnityEngine;

namespace Managers
{
    public class SoundManager : MonoBehaviour
    {
        private static AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public static void OnUIPressed()
        {
            _audioSource.Play();
        }
    }
}