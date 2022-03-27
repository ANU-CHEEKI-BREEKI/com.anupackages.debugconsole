using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using IngameDebug.Pooling;
using IngameDebug.Utils;

namespace IngameDebug.Commands.Console
{
    //TODO: add suggestions class to contain any value
    // or make generic?
    // or make object?
    //
    // need to display commands
    // and command params as well
    //
    // maybe simply make string type of item
    //
    // something like this
    public class Suggestion
    {
        private readonly Func<Suggestion, string, string> _apply;

        public Suggestion(string displayValue, object source, Func<Suggestion, string, string> apply)
        {
            _apply = apply ?? throw new ArgumentNullException(nameof(apply));
            DisplayValue = displayValue;
            Source = source;
        }

        public object Source { get; }
        public string DisplayValue { get; }

        public string ApplySuggestion(string fullInput) => _apply.Invoke(this, fullInput);
    }

    //TODO: suggestion presenter
    // to contain suggestion ipem and display it
    // so maybe suggestion item it just {string, Action} container

    [RequireComponent(typeof(LayoutElement))]
    public class SuggestionPopUp : MonoBehaviour
    {
        [SerializeField] private Transform _parent;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private SuggestionPresenter _suggestionPresenterPrefab;
        [Space]
        [SerializeField] private TextMeshProUGUI _title;
        [Space]
        [SerializeField] private RectTransform[] _contentWidth;

        private readonly Dictionary<Suggestion, SuggestionPresenter> _presenters = new Dictionary<Suggestion, SuggestionPresenter>();
        private readonly LinkedList<Suggestion> _suggestions = new LinkedList<Suggestion>();
        private LinkedListNode<Suggestion> _current;

        public event Action<Suggestion> SelectionChanged;
        public event Action<Suggestion> Choosen;
        public event Action Shown;
        public event Action Hided;

        public string Title
        {
            get => _title.text;
            set => _title.text = value;
        }

        public IEnumerable<Suggestion> Suggestions
        {
            get => _suggestions;
            set
            {
                _suggestions.Clear();
                foreach (var command in value)
                    _suggestions.AddLast(command);
                RegeneratePresenters();
            }
        }

        public bool IsShown => gameObject.activeInHierarchy;

        public Suggestion Selected => _current?.Value;

        private float _preferedSuggestionsElementHeigth;
        private float _preferedSuggestionsElementWidth;
        private LayoutElement _suggestionsElement;
        private RectTransform _suggestionsParent;

        private SimpleGameObjectPool _suggestionsPool;

        public void Deselect()
        {
            _current = null;
            if (IsShown)
                Show();
        }

        public void Show()
        {
            var old = IsShown;
            gameObject.SetActive(true);
            if (old != IsShown)
                Shown?.Invoke();
        }

        public void Hide()
        {
            var old = IsShown;
            gameObject.SetActive(false);
            if (old != IsShown)
                Hided?.Invoke();
        }

        public void MoveUp()
        {
            if (_suggestions.Count == 0)
                return;

            if (_current == null)
                _current = _suggestions.Last;
            else if (_current.Previous != null)
                _current = _current.Previous;
            else
                _current = _suggestions.Last;

            Show();
            ChangeSelection();
        }

        public void MoveDown()
        {
            if (_suggestions.Count == 0)
                return;

            if (_current == null)
                _current = _suggestions.First;
            else if (_current.Next != null)
                _current = _current.Next;
            else
                _current = _suggestions.First;

            Show();
            ChangeSelection();
        }

        public bool TryChooseCurrent()
        {
            if (Selected == null)
                return false;

            if (!_presenters.ContainsKey(Selected))
                return false;

            var presenter = _presenters[Selected];
            if (presenter == null)
                return false;

            presenter.Choose();
            return true;
        }

        private void RegeneratePresenters()
        {
            foreach (var child in _parent.OfType<Transform>().ToList())
                _suggestionsPool.Return(child);

            _presenters.Clear();

            foreach (var suggestion in _suggestions)
            {
                var suggestionPresenter = _suggestionsPool
                    .GetOrCreate()
                    .GetComponent<SuggestionPresenter>();
                suggestionPresenter.transform.SetParent(_parent);

                suggestionPresenter.Present(
                    suggestion,
                    () =>
                    {
                        Deselect();
                        Choosen?.Invoke(suggestion);
                    },
                    () => SelectionChanged?.Invoke(Selected)
                );

                _presenters[suggestion] = suggestionPresenter;
            }
        }

        private void ChangeSelection()
        {
            if (_presenters.ContainsKey(Selected))
                _presenters[Selected]?.Select();

            if (Selected != null)
                ScrollTo(Selected);
        }

        private void ScrollTo(Suggestion suggestion)
        {
            var target = _presenters[suggestion].transform as RectTransform;
            Canvas.ForceUpdateCanvases();

            var contentPanel = _parent as RectTransform;

            var parentRect = (_scrollRect.transform as RectTransform).GetWorldRect();
            var targetRect = target.GetWorldRect();

            if (!targetRect.IsInside(parentRect))
            {
                var shift = Vector2.zero;

                if (targetRect.yMax > parentRect.yMax)
                    shift.y = parentRect.yMax - targetRect.yMax;
                else if (targetRect.yMin < parentRect.yMin)
                    shift.y = parentRect.yMin - targetRect.yMin;

                if (targetRect.xMax > parentRect.xMax)
                    shift.x = parentRect.xMax - targetRect.xMax;
                else if (targetRect.xMin < parentRect.xMin)
                    shift.x = parentRect.xMin - targetRect.xMin;

                var localShift = (Vector2)_scrollRect.transform.InverseTransformVector(shift);
                contentPanel.anchoredPosition += localShift;
            }
        }

        private void Awake()
        {
            _suggestionsPool = new SimpleGameObjectPool(_suggestionPresenterPrefab);

            _suggestionsParent = _suggestionPresenterPrefab.transform.parent as RectTransform;
            _suggestionsElement = this.GetComponent<LayoutElement>();
            _preferedSuggestionsElementHeigth = _suggestionsElement.preferredHeight;
            _preferedSuggestionsElementWidth = _suggestionsElement.preferredWidth;

            _suggestionPresenterPrefab.gameObject.SetActive(false);

            Hide();
        }

        private void LateUpdate()
        {
            if (!IsShown)
                return;

            var position = _suggestionsParent.anchoredPosition;
            position.x = 0;
            _suggestionsParent.anchoredPosition = position;

            _suggestionsElement.preferredHeight = Math.Min(
                _suggestionsParent.rect.height,
                _preferedSuggestionsElementHeigth
            );
            _suggestionsElement.preferredWidth = Math.Min(
                _contentWidth.Sum(r => r.rect.width),
                _preferedSuggestionsElementWidth
            );
        }
    }
}