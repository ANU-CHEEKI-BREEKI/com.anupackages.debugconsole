using System;
using System.Linq;
using ANU.IngameDebug.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
    [DefaultExecutionOrder(100)]
    [ExecuteAlways]
    [DebugCommandPrefix("console")]
    internal class UILogsList : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private UILogPresenter _logPresenterPrefab;
        [SerializeField] private RectTransform _content;
        [SerializeField] private CustomScrollBar _scrollbar;
        [Space]
        [SerializeField] private float _elasticity = 0.1f;
        [SerializeField] private float _elasticityVelocity = 10f;
        [SerializeField] private bool _innertia = true;
        [SerializeField] private float _decelerationRate = 0.135f;
        [Space]
        [SerializeField] private SearchInputField _searchInput;
        [SerializeField] private UIMessageTypeToggle _logs;
        [SerializeField] private UIMessageTypeToggle _warnings;
        [SerializeField] private UIMessageTypeToggle _errors;
        [Space]
        [SerializeField] private Button _scrollToEnd;
        [Space]
        // [SerializeField] private T _field = value;

        private ObjectPool<UILogPresenter> _logsPool;
        private float _velocity;
        private bool _drag;

        private bool _scrollToEndEnabled = true;

        private void Start()
        {
            if (!Application.isPlaying)
                return;

            _logsPool = new UnityEngine.Pool.ObjectPool<UILogPresenter>(
                () => Instantiate(_logPresenterPrefab),
                actionOnGet: p =>
                {
                    p.transform.SetParent(_content, false);
                    p.transform.localScale = Vector3.one;
                    p.transform.localPosition = Vector3.zero;
                    p.transform.localRotation = Quaternion.identity;
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

            DebugConsole.Logs.Cleared += args =>
            {
                foreach (var item in _content.GetComponentsInChildren<UILogPresenter>())
                    _logsPool.Release(item);
                UpdateFilterCounts();
            };
            DebugConsole.Logs.Filtered += args =>
            {
                foreach (var item in _content.GetComponentsInChildren<UILogPresenter>())
                    _logsPool.Release(item);
                UpdateFilterCounts();
            };
            DebugConsole.Logs.Added += args =>
            {
                UpdateFilterCounts();
            };

            _scrollbar.value = 1;
            _scrollbar.size = 1;
            _scrollbar.onValueChanged.AddListener(ScrollNormalize);

            _logs.Toggled += UpdateFilter;
            _warnings.Toggled += UpdateFilter;
            _errors.Toggled += UpdateFilter;
            _searchInput.ValueChanged += UpdateFilter;

            UpdateFilter();

            _scrollToEnd.onClick.AddListener(ScrollToBot);
            _scrollToEndEnabled = true;
        }

        private void OnEnable()
        {
            DebugConsole.ThemeChanged += UpdateTheme;
        }

        private void OnDisable()
        {
            DebugConsole.ThemeChanged -= UpdateTheme;
        }

        private void UpdateTheme(UITheme obj)
        {
            _logs.Color = obj?.Log ?? Color.white;
            _warnings.Color = obj?.Warnings ?? Color.white;
            _errors.Color = obj?.Errors ?? Color.white;
        }

        private void ScrollToBot()
        {
            ScrollNormalize(1);
            _scrollToEnd.gameObject.SetActive(!_scrollToEndEnabled);
            _scrollToEndEnabled = true;
        }

        private void UpdateFilter()
        {
            DebugConsole.Logs.Filter(
                _searchInput.Value,
                Array.Empty<LogType>()
                    .Concat(_logs.Types)
                    .Concat(_warnings.Types)
                    .Concat(_errors.Types)
                    .ToArray()
            );
            UpdateFilterCounts();
        }

        private void UpdateFilterCounts()
        {
            _logs.Count = DebugConsole.Logs.GetMessagesCountFor(LogType.Log);
            _warnings.Count = DebugConsole.Logs.GetMessagesCountFor(LogType.Warning);
            _errors.Count = DebugConsole.Logs.GetMessagesCountFor(LogType.Error)
                + DebugConsole.Logs.GetMessagesCountFor(LogType.Exception)
                + DebugConsole.Logs.GetMessagesCountFor(LogType.Assert);
        }

        private void OnDestroy() => _logsPool?.Dispose();

        private void LateUpdate()
        {
            if (!Application.isPlaying)
                return;

            _scrollToEnd.gameObject.SetActive(!_scrollToEndEnabled);

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
                var nextNode = lastPresenter.Node.Next;
                var lastItemWRect = lastItem.GetWorldRect();

                while (lastItemWRect.yMin > parentWRect.yMin && nextNode != null)
                {
                    var presenter = SpawnPresenter(nextNode);
                    presenter.transform.localPosition = lastItem.localPosition + Vector3.down * lastItemWRect.height * w2LRatio;

                    lastItem = presenter.RectTransform;
                    lastPresenter = presenter;
                    lastItemWRect = lastItem.GetWorldRect();

                    nextNode = nextNode.Next;
                }
            }

            void SpawnTopItems(Rect parentWRect, float w2LRatio)
            {
                if (_content.childCount <= 0)
                    return;

                var firstItem = _content.GetChild(0) as RectTransform;
                var firstPresenter = firstItem.GetComponent<UILogPresenter>();
                var prevNode = firstPresenter.Node.Previous;
                var firstItemWRect = firstItem.GetWorldRect();

                while (firstItemWRect.yMax < parentWRect.yMax && prevNode != null)
                {
                    var presenter = SpawnPresenter(prevNode);
                    presenter.transform.SetAsFirstSibling();

                    firstItem = presenter.RectTransform;
                    firstPresenter = presenter;
                    firstItemWRect = firstItem.GetWorldRect();

                    presenter.transform.localPosition = firstItem.localPosition + Vector3.up * firstItemWRect.height * w2LRatio;

                    prevNode = prevNode.Previous;
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
                    firstIndex = _content.GetChild(0).GetComponent<UILogPresenter>().Node.Value.FilteredIndex;
                    lastIndex = _content.GetChild(_content.childCount - 1).GetComponent<UILogPresenter>().Node.Value.FilteredIndex;
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

                var presenters = _content.GetComponentsInChildren<UILogPresenter>();
                var fullH = presenters.Sum(p => p.RectTransform.GetWorldRect().height);
                var parentH = _content.GetWorldRect().height;

                var firstItem = presenters.First().Node.Previous == null;
                var lastItem = presenters.Last().Node.Next == null;

                // if full items height >= scroll height
                // prior last item
                if (fullH >= parentH && _scrollToEndEnabled)
                {
                    if (lastItem)
                        delta = 1;
                    else if (firstItem)
                        delta = -1;

                    if (presenters.Last().Node.Next != null && _scrollToEndEnabled)
                        delta = 0;
                }
                // if full items height < scroll height
                // prior first item
                else
                {
                    if (firstItem)
                        delta = -1;
                    else if (lastItem)
                        delta = 1;
                }

                CalculateClampDistance(delta, out var perentRect, out var distance);

                if (delta != 0 && (distance > 0 || _scrollToEndEnabled))
                    _velocity = -delta * distance * _elasticityVelocity;
                else if (delta == 0 && _scrollToEndEnabled)
                    _velocity = 5000;
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
            var startNode = DebugConsole.Logs.ElementAtIndexClampedOrDefault(startIndex);
            var nextNode = startNode;

            //if its empty - spawn from top to bot
            var h = 0f;
            while (fullHeight < parentWRect.height && nextNode != null)
            {
                var presenter = SpawnPresenter(nextNode);
                presenter.RectTransform.anchoredPosition = Vector2.zero + Vector2.down * h * w2LRatio;
                var wRect = presenter.RectTransform.GetWorldRect();
                fullHeight += wRect.height;
                h = wRect.height;

                nextNode = nextNode.Next;
            }

            // then from bot to top but from initial start index
            // nad then ne need to scroll all down by this items height
            var prevNode = startNode.Previous;
            var upH = 0f;
            while (fullHeight < parentWRect.height && prevNode != null)
            {
                var presenter = SpawnPresenter(prevNode);
                presenter.transform.SetAsFirstSibling();
                var wRect = presenter.RectTransform.GetWorldRect();
                upH += wRect.height * w2LRatio;
                presenter.RectTransform.anchoredPosition = Vector2.zero + Vector2.up * upH;
                fullHeight += wRect.height;

                prevNode = prevNode.Previous;
            }

            // move down to fit UP items inside parent rect
            for (int i = 0; i < _content.childCount; i++)
                _content.GetChild(i).localPosition += Vector3.down * upH;
        }

        [DebugCommand(Name = "fold-in", Description = "Fold in all messages", DisplayOptions = CommandDisplayOptions.All & ~CommandDisplayOptions.Dashboard)]
        private void Expand(
            [OptDesc("Set this flag to expand all message instead"), OptAltNames("e")]
            bool expand = false
        )
        {
            foreach (var item in DebugConsole.Logs.AllLogs)
                item.IsExpanded = expand;
        }

        [DebugCommand(Name = "filter", Description = "Filter console messages by message type or/and search string", DisplayOptions = CommandDisplayOptions.All & ~CommandDisplayOptions.Dashboard)]
        private void FilterItems(
            [OptDesc("Substring to search in all console messages"), OptAltNames("s")]
            string search = "",
            [OptDesc("Hide info messages"), OptAltNames("i")]
            bool excludeInfo = false,
            [OptDesc("Hide warning messages"), OptAltNames("w")]
            bool excludeWarning = false,
            [OptDesc("Hide error messages"), OptAltNames("e")]
            bool excludeError = false,
            [OptDesc("Inverse opt flags to Negative values. Example: console.filter -nisNullReference\r\nIt means find all info messages with \"NullReference\" substring. Other words - '-ni' means exclude all but info messages"), OptAltNames("n")]
            bool inverseFlags = false
        )
        {
            if (inverseFlags)
            {
                excludeInfo = !excludeInfo;
                excludeWarning = !excludeWarning;
                excludeError = !excludeError;
            }

            _searchInput.Input.SetTextWithoutNotify(search);
            _logs.Toggle.SetIsOnWithoutNotify(!excludeInfo);
            _warnings.Toggle.SetIsOnWithoutNotify(!excludeWarning);
            _errors.Toggle.SetIsOnWithoutNotify(!excludeError);
            UpdateFilter();
        }


        [DebugCommand(Name = "scroll-auto", Description = "Enable auto scroll to last message in the console", DisplayOptions = CommandDisplayOptions.All & ~CommandDisplayOptions.Dashboard)]
        private void AutoScroll(
            [OptDesc("Set this flag to disable auto scroll instead")]
            [OptAltNames("d")]
            bool disable = false
        )
        {
            if (!disable)
                ScrollToBot();
            else
                _scrollToEndEnabled = false;
        }

        [DebugCommand(Name = "scroll-to", Description = "Scroll console to provided normalized position", DisplayOptions = CommandDisplayOptions.All & ~CommandDisplayOptions.Dashboard)]
        private void ScrollNormalize(
            [OptDesc("Normalized position to scroll the console. in rage [0,1]")]
            [OptAltNames("n")]
            [OptVal("0", "0.5", "1")]
            float normalizedPosition
        )
        {
            normalizedPosition = Mathf.Clamp01(normalizedPosition);

            _scrollbar.SetValueWithoutNotify(normalizedPosition);

            _scrollToEndEnabled = false;
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

        private UILogPresenter SpawnPresenter(LogNode node)
        {
            var presenter = _logsPool.Get();
            presenter.Present(
                node,
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

            _scrollToEndEnabled = delta > 0 && distance > 0;
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

            if (Time.deltaTime <= 0)
                return;

            if (_innertia)
                _velocity = eventData.delta.y / Time.deltaTime;
        }
    }
}