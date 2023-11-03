using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    public class CategoryFilterToggle : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private TextMeshProUGUI _label;

        public string Title { get; private set; }
        public Toggle Toggle => _toggle;

        public void Present(string title, ToggleGroup group)
        {
            Title = title;
            _label.text = title;
            _toggle.group = group;
            _toggle.SetIsOnWithoutNotify(false);
        }
    }
}