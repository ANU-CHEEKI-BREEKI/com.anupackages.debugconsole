using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    public class DashboardGroup : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private DashboardLayout _layout;

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(isOn =>
            {
                _layout.enabled = isOn;
                foreach (var item in _layout.RectChildren)
                    item.gameObject.SetActive(isOn);
            });
        }
    }
}