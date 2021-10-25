using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IngameDebug.Utils
{
    public abstract class Singletone<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        protected static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = GameObject.FindObjectOfType<T>();

                if (_instance == null)
                {
                    var holder = new GameObject(typeof(T).Name);
                    holder.AddComponent<T>();
                    DontDestroyOnLoad(_instance.gameObject);
                }

                if (_instance is Singletone<T> singletone && !singletone._isInitialized)
                    singletone.IntarnalInitialize();

                return _instance;
            }
        }

        private bool _isInitialized = false;

        protected void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }

            if (_instance == null)
                _instance = this as T;

            if (_instance is Singletone<T> singletone && !singletone._isInitialized)
                singletone.IntarnalInitialize();
        }

        private void IntarnalInitialize()
        {
            _isInitialized = true;
            Initialize();
        }

        protected abstract void Initialize();
    }
}