using System;
using System.Linq;
using ANU.IngameDebug.Console.Commands.Implementations;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal class ToggleCommandPresenter : CommandPresenterBase
    {
        [SerializeField] private Toggle _toggle;

        private MemberCommand _command;

        public override void Initialize(InitArgs initArgs)
        {
            base.Initialize(initArgs);
            
            _toggle.onValueChanged.AddListener(Toggle);
        }

        private void Toggle(bool isOn)
            => DebugConsole.ExecuteCommand($"{_command.Name} \"{isOn}\"");

        protected override void PresentInternal(MemberCommand command)
        {
            _command = command;
            var initValue = false;
            try
            {
                initValue = (bool)command.Execute().ReturnValues.First().ReturnValue;
            }
            finally { }
            _toggle.SetIsOnWithoutNotify(initValue);
        }
    }
}