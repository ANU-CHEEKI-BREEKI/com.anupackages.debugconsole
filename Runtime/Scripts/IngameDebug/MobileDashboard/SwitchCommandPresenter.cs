using System.Collections.Generic;
using System.Linq;
using ANU.IngameDebug.Console.Commands.Implementations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    [RequireComponent(typeof(LayoutElement))]
    internal class SwitchCommandPresenter : CommandPresenterBase
    {
        [SerializeField] private ToggleGroup _group;
        [SerializeField] private List<Toggle> _toggles;

        private LayoutElement _layout;

        private MemberCommand _command;

        protected override void Awake()
        {
            base.Awake();

            _group.allowSwitchOff = false;
            foreach (var item in _toggles)
                item.group = _group;

            foreach (var item in _toggles)
                item.onValueChanged.AddListener(Toggle);

            _layout = GetComponent<LayoutElement>();
        }

        protected override void PresentInternal(MemberCommand command)
        {
            _command = command;
            var defaultValue = _command.ParametersCache[0].DefaultValue;
            var str = DebugConsole.Converters.ConvertToString(defaultValue);
            var values = _command.ValueHints.Values.First().ToList();
            for (int i = 0; i < _toggles.Count; i++)
            {
                _toggles[i].gameObject.SetActive(i <= values.Count - 1);
                if (_toggles[i].gameObject.activeSelf)
                    _toggles[i].GetComponentInChildren<TextMeshProUGUI>().text = values[i];
            }
            var index = values.IndexOf(str);
            SetIsOnWithoutNotify(index);

            _layout.minWidth = Mathf.Max(500, values.Count * 162);
            _layout.flexibleWidth = Mathf.Max(20, values.Count * 7.5f);
        }

        private void SetIsOnWithoutNotify(int index)
        {
            foreach (var item in _toggles)
                item.SetIsOnWithoutNotify(false);
            _toggles[index].SetIsOnWithoutNotify(true);
        }

        private void Toggle(bool isOn)
        {
            var index = _toggles.IndexOf(_toggles.First(t => t.isOn));
            var value = _command.ValueHints.First().Value.Skip(index).Take(1).Single();
            DebugConsole.ExecuteCommand($"{_command.Name} {value}");
        }
    }
}