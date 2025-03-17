using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Utils
{
    public class ObjectPool : Singleton<ObjectPool>
    {
        [SerializeField] private FriendlyNameHandler ObjectToPool;
        [SerializeField] private int InitialPoolSize = 10;
        private readonly List<FriendlyNameHandler> _pooledObjects = new();

        private void OnEnable()
        {
            for (int i = 0; i < InitialPoolSize; i++)
            {
                FriendlyNameHandler newInstance = Instantiate(ObjectToPool);
                newInstance.gameObject.SetActive(false);
                _pooledObjects.Add(newInstance);
            }
        }

        public FriendlyNameHandler GetPooledObject(Transform parent)
        {
            FriendlyNameHandler pooledObject;
            if (_pooledObjects.Count > 0)
            {
                pooledObject = _pooledObjects[0];
                _pooledObjects.RemoveAt(0);
                pooledObject.transform.SetParent(parent, false);
            }
            else
            {
                pooledObject = Instantiate(ObjectToPool, parent, false);
            }
            pooledObject.gameObject.SetActive(true);
            return pooledObject;
        }

        public void ReturnObjectToPool(FriendlyNameHandler pooledObject)
        {
            pooledObject.gameObject.SetActive(false);
            _pooledObjects.Add(pooledObject);
        }

        public void ReturnObjectsToPool(List<FriendlyNameHandler> entityPanels)
        {
            foreach (FriendlyNameHandler pooledObject in entityPanels)
            {
                pooledObject.gameObject.SetActive(false);
                _pooledObjects.Add(pooledObject);
            }
            entityPanels.Clear();
        }
    }
}