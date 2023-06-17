using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace ANU.IngameDebug.Console
{
    public class UILogsList : MonoBehaviour, IDragHandler
    {
        [SerializeField] private UILogPresenter _logPresenterPrefab;
        [SerializeField] private Transform _content;
        [SerializeField] private RectTransform _viewPort;

        private ObjectPool<UILogPresenter> _logsPool;
        private List<UILogPresenter> _presenters = new();

        private void Awake()
        {
            _logsPool = new UnityEngine.Pool.ObjectPool<UILogPresenter>(
                () => Instantiate(_logPresenterPrefab),
            );

            DebugConsole.Logs.Changed += Changed;

            while (_content.childCount > 0)
            {
                var c = _content.GetChild(0);
                c.SetParent(null);
                Destroy(c.gameObject);
            }
        }

        private void OnDestroy()
        {
            DebugConsole.Logs.Changed -= Changed;
            _logsPool.Dispose();
        }

        private void Changed(LogsContainer.CollectionChangedArgs obj)
        {
            foreach (var log in obj.ChangedItems)
            {
                if (obj.Type == LogsContainer.CollectionChangedArgs.ChangeType.Add)
                {
                    var p = _logsPool.Get();
                    p.transform.SetParent(_content);
                    p.Present(log);
                    _presenters.Add(p);
                }
                else // remove
                {
                    foreach (var item in _presenters.Where(p => p.Log == log).ToArray())
                    {
                        item.transform.SetParent(null);
                        Destroy(item.gameObject);
                        _presenters.Remove(item);
                    }
                }
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }
    }
}