using System.Collections.Generic;
using System.Linq;
using ANU.IngameDebug.Console.Commands.Implementations;
using ANU.IngameDebug.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal class SwitchCommandPresenter : CommandPresenterBase
    {
        [SerializeField] private ToggleGroup _group;
        [SerializeField] private List<Toggle> _toggles;

        private MemberCommand _command;
        private bool _isInitializing;
        private bool _justEnabled;

        protected override void OnEnable()
        {
            base.OnEnable();
            _justEnabled = true;
            this.InvokeSkipOneFrame(() => _justEnabled = false);
        }

        public override void Initialize(InitArgs initArgs)
        {
            base.Initialize(initArgs);

            _group.allowSwitchOff = false;
            foreach (var item in _toggles)
                item.group = _group;

            foreach (var item in _toggles)
                item.onValueChanged.AddListener(Toggle);
        }

        protected override void PresentInternal(MemberCommand command)
        {
            _isInitializing = true;
            _command = command;

            var values = _command.ValueHints.Values.First().ToList();
            for (int i = 0; i < _toggles.Count; i++)
            {
                _toggles[i].gameObject.SetActive(i <= values.Count - 1);
                if (_toggles[i].gameObject.activeSelf)
                    _toggles[i].GetComponentInChildren<TextMeshProUGUI>().text = values[i];
            }

            if (command is MethodCommand)
            {
                _group.allowSwitchOff = true;
                DeselectAll();
            }
            else
            {
                _group.allowSwitchOff = false;

                object initValue = default;
                try
                {
                    initValue = DebugConsole.ExecuteCommand(command.Name, silent: true).ReturnValues.First().ReturnValue;
                }
                catch { }

                var str = DebugConsole.Converters.ConvertToString(initValue);

                var index = values.IndexOf(str);
                SetIsOnWithoutNotify(index);
            }

            _isInitializing = false;
        }

        private void DeselectAll()
        {
            foreach (var item in _toggles)
                item.SetIsOnWithoutNotify(false);
        }
        private void SetIsOnWithoutNotify(int index)
        {
            DeselectAll();

            if (index >= 0 && index < _toggles.Count)
                _toggles[index].SetIsOnWithoutNotify(true);
        }

        private void Toggle(bool isOn)
        {
            if (_justEnabled)
            {
                _justEnabled = false;
                return;
            }

            if (!isOn || _isInitializing)
                return;

            var index = _toggles.IndexOf(_toggles.First(t => t.isOn));
            var value = _command.ValueHints.First().Value.Skip(index).Take(1).Single();
            DebugConsole.ExecuteCommand($"{_command.Name} \"{value}\"");

            if (_command is MethodCommand)
                DeselectAll();
        }

        protected override void UpdateLayout(LayoutElement layout)
        {
            var values = _command.ValueHints.Values.First().ToList();
            layout.minWidth = Mathf.Max(500, values.Count * 162);
            layout.flexibleWidth = Mathf.Max(20, values.Count * 7.5f);
        }
    }
}