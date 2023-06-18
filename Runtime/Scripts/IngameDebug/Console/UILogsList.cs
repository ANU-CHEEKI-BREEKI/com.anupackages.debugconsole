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
    [DefaultExecutionOrder(100)]
    public class UILogsList : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private UILogPresenter _logPresenterPrefab;
        [SerializeField] private RectTransform _content;
        [SerializeField] private CustomScrollBar _scrollbar;
        [Space]
        [SerializeField] private float _elasticity = 0.1f;
        [SerializeField] private bool _innertia = true;
        [SerializeField] private float _decelerationRate = 0.135f;

        private ObjectPool<UILogPresenter> _logsPool;
        private float _velocity;
        private bool _drag;

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

            while (_content.childCount > 0)
            {
                var c = _content.GetChild(0);
                c.SetParent(null);
                Destroy(c.gameObject);
            }

            DebugConsole.Logs.Changed += args =>
            {
                if (args.Type != CollectionChangedArgs.ChangeType.Remove)
                    return;

                foreach (var item in _content.GetComponentsInChildren<UILogPresenter>())
                {
                    if (args.ChangedItems.Contains(item.Log))
                        _logsPool.Release(item);
                };
            };

            _scrollbar.value = 0;
            _scrollbar.size = 1;
            _scrollbar.onValueChanged.AddListener(ScrollNormalize);
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

        private void LateUpdate()
        {
            // if some item above or below container - disable it and adjust content position
            var parentWRect = _content.GetWorldRect();
            var parentLRect = _content.rect;
            var w2LRatio = Mathf.Approximately(parentWRect.height, 0)
                ? 1
                : parentLRect.height / parentWRect.height;

            Layout(parentWRect);
            DisableOutOrParentRect(parentWRect);

            // spawn items until there are free space in the container
            SpawnBotItems(parentWRect, w2LRatio);
            SpawnTopItems(parentWRect, w2LRatio);
            SpawnIfNoItems(parentWRect, w2LRatio);
            CalculateClampVelocity();
            Scroll();
            UpdateScrollBar();

            void Layout(Rect parentWRect)
            {
                var epsilon = 1f;
                for (int i = 0; i < _content.childCount - 1; i++)
                {
                    var c = _content.GetChild(i) as RectTransform;
                    var c2 = _content.GetChild(i + 1) as RectTransform;
                    var itemWRect = c.GetWorldRect();
                    var itemWRect2 = c2.GetWorldRect();

                    var offset = 0f;

                    if (itemWRect.yMin < itemWRect2.yMax)
                        offset = (itemWRect2.yMax - itemWRect.yMin) * w2LRatio;
                    else if (itemWRect.yMin - itemWRect2.yMax > epsilon)
                        offset = -(itemWRect.yMin - itemWRect2.yMax) * w2LRatio;

                    if (!Mathf.Approximately(offset, 0))
                    {
                        for (int j = i + 1; j < _content.childCount; j++)
                            _content.GetChild(j).localPosition += Vector3.down * offset;
                    }
                }
            }

            void DisableOutOrParentRect(Rect parentWRect)
            {
                for (int i = 0; i < _content.childCount; i++)
                {
                    var c = _content.GetChild(i) as RectTransform;
                    var itemWRect = c.GetWorldRect();

                    if (itemWRect.IsOutside(parentWRect, 1))
                    {
                        // calculate sized above and below center
                        var a = Mathf.Max(0, Mathf.Min(itemWRect.height, itemWRect.yMax - parentWRect.center.y));
                        var b = Mathf.Max(0, Mathf.Min(itemWRect.height, parentWRect.center.y - itemWRect.yMin));

                        // if (a > 0)
                        //     _index++;
                        // if (b > 0)
                        //     _index--;

                        i--;
                        _logsPool.Release(c.GetComponent<UILogPresenter>());
                    }
                }
            }

            void SpawnBotItems(Rect parentWRect, float w2LRatio)
            {
                if (_content.childCount <= 0)
                    return;

                var lastItem = _content.GetChild(_content.childCount - 1) as RectTransform;
                var lastPresenter = lastItem.GetComponent<UILogPresenter>();
                var lastIndex = lastPresenter.Index;
                var lastItemWRect = lastItem.GetWorldRect();

                while (lastItemWRect.yMin > parentWRect.yMin && lastIndex < DebugConsole.Logs.Count - 1)
                {
                    lastIndex++;

                    var presenter = SpawnPresenter(lastIndex);
                    presenter.transform.localPosition = lastItem.localPosition + Vector3.down * lastItemWRect.height * w2LRatio;

                    lastItem = presenter.RectTransform;
                    lastPresenter = presenter;
                    lastItemWRect = lastItem.GetWorldRect();
                }
            }

            void SpawnTopItems(Rect parentWRect, float w2LRatio)
            {
                if (_content.childCount <= 0)
                    return;

                var firstItem = _content.GetChild(0) as RectTransform;
                var firstPresenter = firstItem.GetComponent<UILogPresenter>();
                var firstIndex = firstPresenter.Index;
                var firstItemWRect = firstItem.GetWorldRect();

                while (firstItemWRect.yMax < parentWRect.yMax && firstIndex > 0)
                {
                    firstIndex--;

                    var presenter = SpawnPresenter(firstIndex);
                    presenter.transform.SetAsFirstSibling();

                    firstItem = presenter.RectTransform;
                    firstPresenter = presenter;
                    firstItemWRect = firstItem.GetWorldRect();

                    presenter.transform.localPosition = firstItem.localPosition + Vector3.up * firstItemWRect.height * w2LRatio;
                }
            }

            void UpdateScrollBar()
            {
                if (_scrollbar.IsDragging)
                    return;

                var range = 0;
                var firstIndex = 0;
                var lastIndex = 0;
                if (_content.childCount > 0)
                {
                    firstIndex = _content.GetChild(0).GetComponent<UILogPresenter>().Index;
                    lastIndex = _content.GetChild(_content.childCount - 1).GetComponent<UILogPresenter>().Index;
                    range = lastIndex - firstIndex;
                }

                _scrollbar.size = DebugConsole.Logs.Count == 0
                    ? 1f
                    : range / (float)DebugConsole.Logs.Count;

                _scrollbar.SetValueWithoutNotify(DebugConsole.Logs.Count == 0
                    ? 0f
                    : firstIndex / (float)(DebugConsole.Logs.Count - range)
                );
            }

            void CalculateClampVelocity()
            {
                if (_drag)
                    return;

                if (_content.childCount <= 0)
                    return;

                var delta = 0;
                if (_content.GetChild(0).GetComponent<UILogPresenter>().Index == 0)
                    delta = -1;
                else if (_content.GetChild(_content.childCount - 1).GetComponent<UILogPresenter>().Index == DebugConsole.Logs.Count - 1)
                    delta = 1;

                if (delta == 0)
                    return;

                CalculateClampDistance(delta, out var perentRect, out var distance);

                if (distance < 0)
                    return;

                _velocity = -delta * distance * 10;
            }

            void Scroll()
            {
                if (_drag)
                    return;

                _velocity = Mathf.Lerp(_velocity, 0, _decelerationRate * Time.deltaTime);

                for (int i = 0; i < _content.childCount; i++)
                    _content.GetChild(i).localPosition += Vector3.up * _velocity * Time.deltaTime;
            }
        }

        void SpawnIfNoItems(Rect parentWRect, float w2LRatio)
        {
            if (_content.childCount > 0)
                return;

            var startIndex = Mathf.RoundToInt(
                _scrollbar.value * DebugConsole.Logs.Count
            );

            var fullHeight = 0f;

            //if its empty - spawn from top to bot
            // and then move lil up to scroll down to the end
            var nextIndex = startIndex;
            var h = 0f;
            while (fullHeight < parentWRect.height && nextIndex < DebugConsole.Logs.Count)
            {
                var presenter = SpawnPresenter(nextIndex);
                presenter.RectTransform.anchoredPosition = Vector2.zero + Vector2.down * h * w2LRatio;
                var wRect = presenter.RectTransform.GetWorldRect();
                fullHeight += wRect.height;
                h = wRect.height;

                nextIndex++;
            }

            var prevIndex = startIndex - 1;
            var upH = 0f;
            while (fullHeight < parentWRect.height && prevIndex < DebugConsole.Logs.Count && prevIndex > 0)
            {
                var presenter = SpawnPresenter(prevIndex);
                var wRect = presenter.RectTransform.GetWorldRect();
                presenter.RectTransform.anchoredPosition = Vector2.zero + Vector2.up * wRect.height * w2LRatio;
                fullHeight += wRect.height;
                upH += wRect.height;

                prevIndex++;
            }

            // move down to fir UP items inside parent rect
            for (int i = 0; i < _content.childCount; i++)
                _content.GetChild(i).localPosition += Vector3.down * upH * w2LRatio;
        }

        private void ScrollNormalize(float normalizedPosition)
        {
            // WHEN scrollbar set value by ui
            // we can release all child, and then refill from start index from bot to top.. 
            // like when we initializing the scroll. but from provided start index instead of 0

            _velocity = 0;
            while (_content.childCount > 0)
            {
                _logsPool.Release(
                    _content.GetChild(0).GetComponent<UILogPresenter>()
                );
            }

            var parentWRect = _content.GetWorldRect();
            var parentLRect = _content.rect;
            var w2LRatio = Mathf.Approximately(parentWRect.height, 0)
                ? 1
                : parentLRect.height / parentWRect.height;

            SpawnIfNoItems(parentLRect, w2LRatio);
        }

        private UILogPresenter SpawnPresenter(int index)
        {
            var log = DebugConsole.Logs[index];
            var presenter = _logsPool.Get();
            presenter.Present(
                index,
                log,
                onClick: () =>
                {
                    presenter.RectTransform.ForceUpdateRectTransforms();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(presenter.RectTransform);
                }
            );
            presenter.RectTransform.ForceUpdateRectTransforms();
            LayoutRebuilder.ForceRebuildLayoutImmediate(presenter.RectTransform);
            var sd = presenter.RectTransform.sizeDelta;
            sd.x = 0;
            presenter.RectTransform.anchoredPosition = Vector2.zero;
            presenter.RectTransform.sizeDelta = sd;
            return presenter;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            _velocity = 0;

            if (_content.childCount <= 0)
                return;

            var delta = eventData.delta.y;
            CalculateClampDistance(delta, out var parentrect, out var distance);

            if (distance > parentrect.height * _elasticity)
                delta *= parentrect.height * _elasticity / distance * 0.5f;

            for (int i = 0; i < _content.childCount; i++)
                _content.GetChild(i).localPosition += Vector3.up * delta;
        }

        private void CalculateClampDistance(float delta, out Rect parentrect, out float distance)
        {
            var scrollDown = delta > 0;

            var borderItem = _content.GetChild(scrollDown ? _content.childCount - 1 : 0) as RectTransform;
            var wRect = borderItem.GetWorldRect();
            parentrect = _content.GetWorldRect();
            distance = scrollDown
                ? wRect.yMin - parentrect.yMin
                : parentrect.yMax - wRect.yMax;
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _drag = true;
            _velocity = 0;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            _drag = false;
            if (_innertia)
                _velocity = eventData.delta.y / Time.deltaTime;
        }
    }
}