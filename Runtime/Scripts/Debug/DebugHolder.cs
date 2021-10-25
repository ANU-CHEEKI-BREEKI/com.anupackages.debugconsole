using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IngameDebug
{
    public class DebugHolder : MonoBehaviour
    {
        private static DebugHolder _instance;

        [SerializeField] private GameObject _holder;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
#if DEBUG
                _holder.SetActive(true);
                return;
#endif
#pragma warning disable CS0162
                gameObject.SetActive(false);
#pragma warning restore CS0162
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}