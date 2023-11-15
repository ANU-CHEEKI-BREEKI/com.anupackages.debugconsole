using System.Linq;
using ANU.IngameDebug.Console.Commands.Implementations;
using ANU.IngameDebug.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal class InputCommandPresenter : CommandPresenterBase
    {
        [SerializeField] private TMP_InputField _input;
        [SerializeField] private Button _button;

        private MemberCommand _command;

        public override void Initialize(InitArgs initArgs)
        {
            base.Initialize(initArgs);

            _button.onClick.AddListener(Execute);
            _input.onSubmit.AddListener(Execute);
        }

        protected override void PresentInternal(MemberCommand command)
        {
            _command = command;

            if (command is MethodCommand)
            {
                _input.SetTextWithoutNotify(
                    DebugConsole.Converters.ConvertToString(command.ParametersCache[0].DefaultValue)
                );
            }
            else
            {
                object initValue = default;
                try
                {
                    initValue = DebugConsole.ExecuteCommand(command.Name, silent: true).ReturnValues.First().ReturnValue;
                }
                catch { }

                _input.SetTextWithoutNotify(
                    DebugConsole.Converters.ConvertToString(initValue)
                );
            }

            _input.contentType = command.ParametersCache[0].DefaultValue.GetContentType();
        }

        private void Execute(string arg0) => Execute();
        private void Execute() => DebugConsole.ExecuteCommand($"{_command.Name} \"{_input.text}\"");

        protected override void UpdateLayout(LayoutElement layout)
        {
            base.UpdateLayout(layout);

            if (_input.contentType == TMP_InputField.ContentType.IntegerNumber
                || _input.contentType == TMP_InputField.ContentType.DecimalNumber)
            {

            }
            else
            {
                layout.flexibleWidth *= 2f;
                layout.minWidth *= 1.5f;
            }
        }
    }
}