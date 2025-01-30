using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Utils
{
    public class ObjectPool : Singleton<ObjectPool>
    {
        [SerializeField] private FriendlyNameHandler ObjectToPool;
        private readonly List<FriendlyNameHandler> _pooledObjects = new();

        public FriendlyNameHandler GetPooledObject()
        {
            if (_pooledObjects.Count > 0)
            {
                FriendlyNameHandler pooledObject = _pooledObjects[0];
                _pooledObjects.RemoveAt(0);
                pooledObject.gameObject.SetActive(true);
                return pooledObject;
            }
            else
            {
                return Instantiate(ObjectToPool);
            }
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