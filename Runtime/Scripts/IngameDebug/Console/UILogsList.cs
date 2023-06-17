using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;
using static ANU.IngameDebug.Console.LogsContainer;

namespace ANU.IngameDebug.Console
{
    public class UILogsList : MonoBehaviour
    {
        [SerializeField] private UILogPresenter _logPresenterPrefab;
        [SerializeField] private RectTransform _content;
        [SerializeField] private Scrollbar _scrollbar;

        private ObjectPool<UILogPresenter> _logsPool;
        private List<UILogPresenter> _presenters = new();
        private bool _changed;

        private void Awake()
        {
            _logsPool = new UnityEngine.Pool.ObjectPool<UILogPresenter>(
                () => Instantiate(_logPresenterPrefab),
                actionOnGet: p => p.gameObject.SetActive(true),
                actionOnRelease: p => p.gameObject.SetActive(false)
            );

            DebugConsole.Logs.Changed += Changed;

            while (_content.childCount > 0)
            {
                var c = _content.GetChild(0);
                c.SetParent(null);
                Destroy(c.gameObject);
            }

            _scrollbar.value = 1f;
            _scrollbar.size = 0.1f;
        }

        private void OnEnable()
        {
            //FIXME: layout broken on first open
            
            //TODO: disable nested canvas to disable rendering
            // then rebuild layout and then enable canvas
            // to not show lags
            // maybe display some loading icon

            // foreach (var p in _presenters)
            //     LayoutRebuilder.ForceRebuildLayoutImmediate(p.RectTransform);
            // LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        }

        private void Start()
        {
            // fill debug logs
            var LongText = "\r\nLorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Elementum integer enim neque volutpat ac tincidunt vitae. Massa tempor nec feugiat nisl pretium fusce id. Tincidunt augue interdum velit euismod in pellentesque massa placerat. At lectus urna duis convallis convallis tellus id interdum velit. Sit amet mattis vulputate enim nulla aliquet porttitor. Non arcu risus quis varius quam quisque id diam. Interdum posuere lorem ipsum dolor sit amet consectetur adipiscing elit. Vestibulum morbi blandit cursus risus at. Ac placerat vestibulum lectus mauris ultrices eros in cursus. Libero justo laoreet sit amet. Praesent elementum facilisis leo vel. Blandit libero volutpat sed cras. Gravida in fermentum et sollicitudin ac.";

            var ShortText = "Lorem ipsum dolor sit amet";

            for (int i = 0; i < 500; i++)
                Debug.Log($" --->   {i}   <--- {(i % 10 == 0 && UnityEngine.Random.value < 0.2f ? LongText : ShortText)}");
        }

        private void OnDestroy()
        {
            _logsPool.Dispose();
        }

        private void Changed(CollectionChangedArgs obj)
        {
            foreach (var log in obj.ChangedItems)
            {
                if (obj.Type == LogsContainer.CollectionChangedArgs.ChangeType.Add)
                {
                    var p = _logsPool.Get();
                    p.transform.SetParent(_content, false);
                    p.Present(
                        log,
                        onClick: () => LayoutRebuilder.ForceRebuildLayoutImmediate(p.RectTransform)
                    );
                    _presenters.Add(p);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(p.RectTransform);
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

            // // release all
            // for (int i = 0; i < _presenters.Count; i++)
            //     _logsPool.Release(_presenters[i]);
            // _presenters.Clear();

            // if (DebugConsole.Logs.Count <= 0)
            //     return;

            // // rebind new
            // var index = Mathf.RoundToInt(DebugConsole.Logs.Count * (1 - _scrollbar.value));
            // // take slice of logs 
            // var containerHeight = _content.rect.height;

            // var itemsHeight = 0f;
            // var direction = 1;
            // var t = 0;
            // var lowBoundary = false;
            // var highBoundary = false;

            // do
            // {
            //     var i = index + direction * t;

            //     if (i < 0)
            //         lowBoundary = true;

            //     if (i >= DebugConsole.Logs.Count)
            //         highBoundary = true;

            //     if (i >= 0 && i < DebugConsole.Logs.Count)
            //     {
            //         var log = DebugConsole.Logs[i];
            //         var p = _logsPool.Get();
            //         p.transform.SetParent(_content, false);
            //         p.Present(log);
            //         itemsHeight += p.PrefferedHeight;

            //         if (direction <= 0)
            //             _presenters.Insert(0, p);
            //         else
            //             _presenters.Add(p);
            //     }
            //     direction = (int)Mathf.Sign(direction) * -1;
            //     if (direction < 0)
            //         t++;
            // }
            // while (itemsHeight < containerHeight * 2 && _presenters.Count < DebugConsole.Logs.Count && (!lowBoundary || !highBoundary));

            // DebugConsole.ShowLogs = false;

            // if (itemsHeight >= containerHeight * 2)
            //     Debug.Log("itemsHeight >= containerHeight * 2");

            // if (_presenters.Count >= DebugConsole.Logs.Count)
            //     Debug.Log("_presenters.Count >= DebugConsole.Logs.Count");

            // if (lowBoundary && highBoundary)
            //     Debug.Log("lowBoundary && highBoundary");

            // DebugConsole.ShowLogs = true;

            // var h = 0f;
            // var s = 0;
            // foreach (var item in _presenters)
            // {
            //     var p = item.RectTransform.localPosition;
            //     p.y = h;
            //     item.RectTransform.localPosition = p;
            //     h -= item.PrefferedHeight;
            //     item.transform.SetSiblingIndex(s);
            //     s++;
            // }
        }

        // void IDragHandler.OnDrag(PointerEventData eventData)
        // {
        //     for (int i = 0; i < _presenters.Count; i++)
        //     {
        //         _presenters[i].RectTransform.position += eventData.delta.y * Vector3.up;
        //     }
        // }

        // private void LateUpdate()
        // {
        //     if (_changed)
        //     {
        //         _changed = false;
        //         Changed();

        //         //FIXME: somehow update ContentSizeFitter of all visible items
        //     }
        // }
    }
}