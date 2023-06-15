using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console
{
    public class SearchInputField : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _input;
        [SerializeField] private Button _clear;

        private void Awake()
        {
            _clear.onClick.AddListener(() => _input.text = "");
            _input.onValueChanged.AddListener(v => UpdateButtonVisibility());
            UpdateButtonVisibility();
        }

        private void UpdateButtonVisibility()
            => _clear.gameObject.SetActive(!string.IsNullOrEmpty(_input.text));
    }
}