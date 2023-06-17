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
        private bool _changed;
        private int _index;

        private void Awake()
        {
            _logsPool = new UnityEngine.Pool.ObjectPool<UILogPresenter>(
                () => Instantiate(_logPresenterPrefab),
                actionOnGet: p =>
                {
                    p.transform.SetParent(_content);
                    p.transform.localScale = Vector3.one;
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
            return;

            foreach (var log in obj.ChangedItems)
            {
                if (obj.Type == LogsContainer.CollectionChangedArgs.ChangeType.Add)
                {
                    var p = _logsPool.Get();
                    p.Present(
                        log,
                        onClick: () => LayoutRebuilder.ForceRebuildLayoutImmediate(p.RectTransform)
                    );
                    LayoutRebuilder.ForceRebuildLayoutImmediate(p.RectTransform);
                }
                else // remove
                {
                    foreach (var item in _content.GetComponentsInChildren<UILogPresenter>().Where(p => p.Log == log))
                        _logsPool.Release(item);
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
            var contentW2LRatio = Mathf.Approximately(contentWRect.height, 0)
                ? 1
                : contentLRect.height / contentWRect.height;

            for (int i = 0; i < _content.childCount; i++)
            {
                var c = _content.GetChild(i) as RectTransform;
                var itemWRect = c.GetWorldRect();

                if (itemWRect.IsOutside(parentWRect, 1))
                {
                    // calculate sized above and below center
                    var a = Mathf.Max(0, Mathf.Min(itemWRect.height, itemWRect.yMax - parentWRect.center.y));
                    var b = Mathf.Max(0, Mathf.Min(itemWRect.height, parentWRect.center.y - itemWRect.yMin));
                    hAboveCenter += a;
                    hBelowCenter += b;

                    if (a > 0)
                        _index++;
                    if (b > 0)
                        _index--;

                    i--;
                    _logsPool.Release(c.GetComponent<UILogPresenter>());
                }
            }

            var g = _content.GetComponent<LayoutGroup>();
            var p = g.padding;
            p.top += (int)(hAboveCenter * contentW2LRatio);
            p.bottom += (int)(hBelowCenter * contentW2LRatio);

            // spawn items until there are free space in the container
            var startIndex = 0;
            if (_content.childCount > 0)
                startIndex = _content.GetChild(0).GetComponent<UILogPresenter>().Index - 1;

            while (p.top > 0 && startIndex > 0)
            {
                var log = DebugConsole.Logs[startIndex];
                var presenter = _logsPool.Get();
                presenter.Present(
                    log,
                    onClick: () => LayoutRebuilder.ForceRebuildLayoutImmediate(presenter.RectTransform)
                );
                LayoutRebuilder.ForceRebuildLayoutImmediate(presenter.RectTransform);
                p.top -= (int)presenter.PrefferedHeight;

                startIndex--;
            }

            startIndex = 0;
            if (_content.childCount > 0)
                startIndex = _content.GetChild(_content.childCount - 1).GetComponent<UILogPresenter>().Index + 1;

            while (p.bottom > 0 && startIndex < DebugConsole.Logs.Count - 1)
            {
                var log = DebugConsole.Logs[startIndex];
                var presenter = _logsPool.Get();
                presenter.Present(
                    log,
                    onClick: () => LayoutRebuilder.ForceRebuildLayoutImmediate(presenter.RectTransform)
                );
                LayoutRebuilder.ForceRebuildLayoutImmediate(presenter.RectTransform);
                p.bottom -= (int)presenter.PrefferedHeight;

                startIndex++;
            }

            // and adjust content position after rebuild layout
            g.padding = p;

            var contentH = contentWRect.height;
            startIndex = 0;
            if (_content.childCount > 0)
                startIndex = _content.GetChild(_content.childCount - 1).GetComponent<UILogPresenter>().Index + 1;

            while (contentH < parentWRect.height && startIndex < DebugConsole.Logs.Count - 1)
            {
                var log = DebugConsole.Logs[startIndex];
                var presenter = _logsPool.Get();
                presenter.Present(
                    log,
                    onClick: () => LayoutRebuilder.ForceRebuildLayoutImmediate(presenter.RectTransform)
                );
                LayoutRebuilder.ForceRebuildLayoutImmediate(presenter.RectTransform);
                contentH += (int)presenter.PrefferedHeight / contentW2LRatio;

                startIndex++;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        }
    }
}