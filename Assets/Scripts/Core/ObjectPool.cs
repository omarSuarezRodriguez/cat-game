using System.Collections.Generic;
using UnityEngine;

namespace WhiskerHaven.Core
{
    /// <summary>
    /// Generic component-based object pool. Attach to a manager or use statically.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _available = new();

        public ObjectPool(T prefab, Transform parent, int initialSize = 10)
        {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < initialSize; i++)
                ReturnToPool(CreateNew());
        }

        private T CreateNew()
        {
            var obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            return obj;
        }

        public T Get(Vector3 worldPosition)
        {
            var obj = _available.Count > 0 ? _available.Dequeue() : CreateNew();
            obj.transform.position = worldPosition;
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void ReturnToPool(T obj)
        {
            obj.gameObject.SetActive(false);
            _available.Enqueue(obj);
        }

        public int AvailableCount => _available.Count;
    }
}
