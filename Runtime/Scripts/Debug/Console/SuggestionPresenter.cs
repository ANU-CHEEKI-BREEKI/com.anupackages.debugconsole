using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
    public class SuggestionPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Button _button;
        [SerializeField] private Selectable _selectable;

        private Suggestion _suggestion;
        private Action _selected;

        public Suggestion Suggestion => _suggestion;

        public void Select()
        {
            _selectable.Select();
            _selected?.Invoke();
        }

        public void Present(Suggestion suggestion, Action choosen, Action selected)
        {
            _suggestion = suggestion;
            _selected = selected;

            _label.text = suggestion.DisplayValue;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => choosen?.Invoke());
        }

        public void Choose() => _button.onClick.Invoke();
    }
}