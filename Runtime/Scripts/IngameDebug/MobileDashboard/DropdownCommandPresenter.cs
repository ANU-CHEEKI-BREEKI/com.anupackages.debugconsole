using System.Linq;
using ANU.IngameDebug.Console.Commands.Implementations;
using TMPro;
using UnityEngine;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal class DropdownCommandPresenter : CommandPresenterBase
    {
        [SerializeField] private TMP_Dropdown _dropdown;

        private MemberCommand _command;
        private bool _isInitializing;

        public override void Initialize(InitArgs initArgs)
        {
            base.Initialize(initArgs);

            _dropdown.onValueChanged.AddListener(ValueChanged);
        }

        protected override void PresentInternal(MemberCommand command)
        {
            _isInitializing = true;
            _command = command;

            var values = _command.ValueHints.Values.First().ToList();
            _dropdown.options.Clear();
            _dropdown.options.AddRange(values.Select(v => new TMP_Dropdown.OptionData(v)));

            if (command is MethodCommand)
            {
                _dropdown.options.Insert(0, new TMP_Dropdown.OptionData(""));
                Deselect();
            }
            else
            {
                object initValue = default;
                try
                {
                    initValue = DebugConsole.ExecuteCommand(command.Name, silent: true).ReturnValues.First().ReturnValue;
                }
                catch { }

                var str = DebugConsole.Converters.ConvertToString(initValue);
                var index = values.IndexOf(str);
                _dropdown.SetValueWithoutNotify(index);
            }

            _isInitializing = false;
        }

        private void Deselect() => _dropdown.SetValueWithoutNotify(-1);

        private void ValueChanged(int valueIndex)
        {
            if (_isInitializing)
                return;

            if (_command is MethodCommand)
                valueIndex--;

            if (valueIndex < 0)
                return;

            var value = _command.ValueHints.First().Value.Skip(valueIndex).Take(1).Single();
            DebugConsole.ExecuteCommand($"{_command.Name} \"{value}\"");

            if (_command is MethodCommand)
                Deselect();
        }
    }
}