using System.Collections.Generic;
using UnityEngine;

namespace ANU.IngameDebug.Pooling
{
    public class SimpleGameObjectPool
    {
        private readonly GameObject _prefab;

        private Queue<GameObject> _pool = new Queue<GameObject>();
        private HashSet<GameObject> _instantiated = new HashSet<GameObject>();

        public SimpleGameObjectPool(Component prefab) : this(prefab.gameObject) { }
        public SimpleGameObjectPool(GameObject prefab) => _prefab = prefab;

        public GameObject GetOrCreate()
        {
            GameObject instance = null;

            if (_pool.Count > 0)
            {
                instance = _pool.Dequeue();
            }
            else
            {
                instance = GameObject.Instantiate(_prefab);
                _instantiated.Add(instance);
            }

            instance.SetActive(true);
            return instance;
        }

        public void Return(Component component) => Return(component.gameObject);
        public void Return(GameObject gameObject)
        {
            if (gameObject == null || !_instantiated.Contains(gameObject))
                return;
            _pool.Enqueue(gameObject);
            gameObject.SetActive(false);
        }
    }
}