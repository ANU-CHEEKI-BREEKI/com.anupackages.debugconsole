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

        private void Awake()
        {
            var group = GetComponent<ToggleGroup>();

            group.allowSwitchOff = false;
            _console.group = group;
            _dashboard.group = group;


            _console.isOn = false;
            _dashboard.isOn = false;
            _console.isOn = true;

            _console.onValueChanged.AddListener(isOn => UpdateTabs());
            _dashboard.onValueChanged.AddListener(isOn => UpdateTabs());

            UpdateTabs();
        }

        private void UpdateTabs()
        {
            _consoleObject.SetActive(_console.isOn);
            _dashboardObject.SetActive(_dashboard.isOn);
        }
    }
}