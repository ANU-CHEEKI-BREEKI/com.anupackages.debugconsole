using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    [RequireComponent(typeof(ToggleGroup))]
    public class ConsoleTabs : MonoBehaviour
    {
        [SerializeField] private Toggle _console;
        [SerializeField] private Toggle _dashboard;
        [Space]
        [SerializeField] private GameObject _consoleObject;
        [SerializeField] private GameObject _dashboardObject;

        private int DefaultTab =>
#if !UNITY_EDITOR &&(UNITY_ANDROID || UNITY_IOS)
            1
#else
            0
#endif
            ;

        private int LastTabIndex
        {
            get => PlayerPrefs.GetInt("ConsoleLastTabIndex", DefaultTab);
            set => PlayerPrefs.SetInt("ConsoleLastTabIndex", value);
        }

        private void Awake()
        {
            var group = GetComponent<ToggleGroup>();

            group.allowSwitchOff = false;
            _console.group = group;
            _dashboard.group = group;

            _console.isOn = false;
            _dashboard.isOn = false;

            var targetTab = LastTabIndex == 0
                ? _console
                : _dashboard;
            targetTab.isOn = true;

            _console.onValueChanged.AddListener(isOn => UpdateTabs());
            _dashboard.onValueChanged.AddListener(isOn => UpdateTabs());

            UpdateTabs();
        }

        private void UpdateTabs()
        {
            _consoleObject.SetActive(_console.isOn);
            _dashboardObject.SetActive(_dashboard.isOn);

            LastTabIndex = _console.isOn ? 0 : 1;
        }
    }
}