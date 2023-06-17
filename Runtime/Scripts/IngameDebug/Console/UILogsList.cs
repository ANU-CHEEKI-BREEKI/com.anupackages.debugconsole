using System;
using System.Collections.Generic;
using System.Linq;
using ANU.IngameDebug.Utils;
using TMPro;
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
        [SerializeField] private RectTransform _viewPort;
        [SerializeField] private Scrollbar _scrollbar;

        private ObjectPool<UILogPresenter> _logsPool;
        private List<UILogPresenter> _presenters = new();
        private bool _changed;

        private void Awake()
        {
            _logsPool = new UnityEngine.Pool.ObjectPool<UILogPresenter>(
                () => Instantiate(_logPresenterPrefab),
                actionOnGet: p =>
                {
                    p.transform.SetParent(_content, false);
                    p.gameObject.SetActive(true);
                },
                actionOnRelease: p =>
                {
                    p.gameObject.SetActive(false);
                    p.transform.SetParent(null);
                }
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
        }

        private void LateUpdate()
        {
            // if some item above or below container - disable it and adjust content position
            var viewPort = _viewPort.rect;
            var hAboveCenter = 0f;
            var hBelowCenter = 0f;

            var parentWRect = _viewPort.GetWorldRect();
            var contentWRect = _content.GetWorldRect();
            var contentLRect = _content.rect;
            var contentW2LRatio = contentLRect.x / contentWRect.x;

            for (int i = 0; i < _content.childCount; i++)
            {
                var c = _content.GetChild(i) as RectTransform;
                var itemWRect = c.GetWorldRect();

                // c.GetComponentInChildren<TextMeshProUGUI>().color = itemWRect.IsInside(parentWRect)
                //     ? Color.white
                //     : Color.red;


                // debug text directly in log message
                // var t = c.GetComponentInChildren<TextMeshProUGUI>();
                // if (!t.text.Contains('{'))
                //     t.text += "{-}";
                // t.text = System.Text.RegularExpressions.Regex.Replace(t.text, @"\{.*\}", $"{{{r}}}");
            }
        }

        private void OnDrawGizmos()
        {
            var parentWRect = _viewPort.GetWorldRect();

            for (int i = 0; i < _content.childCount && i <= 0 ; i++)
            {
                var c = _content.GetChild(i) as RectTransform;
                var itemWRect = c.GetWorldRect();

                Extensions.TryGetIntersection(parentWRect, itemWRect, includeZeroSize: false, out var intersection);
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(parentWRect.center, parentWRect.size);
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(itemWRect.center, itemWRect.size);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(intersection.center, intersection.size);
                
                c.GetComponentInChildren<TextMeshProUGUI>().color = itemWRect.IsInside(parentWRect, 1f)
                   ? Color.white
                   : Color.red;

                Debug.Log($"inside: {itemWRect.IsInside(parentWRect)}");
                Debug.Log($"itemWRect, {itemWRect.x}, {itemWRect.y}, {itemWRect.height}, {itemWRect.width}");
                Debug.Log($"intersection, {intersection.x}, {intersection.y}, {intersection.height}, {intersection.width}");
            }
        }

        // get start index from scrollbar
        // spawn items until there are free space in the container
        // and adjust content position after rebuild layout
    }
}