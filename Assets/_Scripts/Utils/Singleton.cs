using UnityEngine;

namespace Utils
{
    /// <summary>
    /// This is a basic singleton. This will destroy any new versions created, leaving the original instance intact
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }
        
        protected virtual void Awake()
        {
            if (Instance == null)
                Instance = this as T;
            else
                Destroy(this);
        }
    }
    
}
