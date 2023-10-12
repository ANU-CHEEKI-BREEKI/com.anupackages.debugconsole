using System;
using System.Linq;
using ANU.IngameDebug.Console.Commands.Implementations;
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
                object initValue;
                try
                {
                    initValue = command.Execute().ReturnValues.First().ReturnValue;
                }
                finally { }

                _input.SetTextWithoutNotify(
                    DebugConsole.Converters.ConvertToString(initValue)
                );
            }

            _input.contentType = command.ParametersCache[0].DefaultValue switch
            {
                short => TMP_InputField.ContentType.IntegerNumber,
                int => TMP_InputField.ContentType.IntegerNumber,
                long => TMP_InputField.ContentType.IntegerNumber,
                ushort => TMP_InputField.ContentType.IntegerNumber,
                uint => TMP_InputField.ContentType.IntegerNumber,
                ulong => TMP_InputField.ContentType.IntegerNumber,
                sbyte => TMP_InputField.ContentType.IntegerNumber,
                byte => TMP_InputField.ContentType.IntegerNumber,

                float => TMP_InputField.ContentType.DecimalNumber,
                double => TMP_InputField.ContentType.DecimalNumber,
                decimal => TMP_InputField.ContentType.DecimalNumber,

                _ => TMP_InputField.ContentType.Standard,
            };
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