using System;
using System.Linq;
using ANU.IngameDebug.Console.Commands;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal class GenericCommandPresenter : CommandPresenterBase
    {
        [SerializeField] private GameObject _requiredParametersSelection;
        [SerializeField] private GameObject _nonRequiredParametersSelection;
        [SerializeField] private Button _button;

        private ADebugCommand _command;

        protected override void Awake()
        {
            base.Awake();
            _button.onClick.AddListener(Execute);
        }

        protected override void PresentInternal(ADebugCommand command)
        {
            _command = command;

            var options = command.Options.Where(c => !c.GetNames().Any(n => n == "h" || n == "t"));
            var anyOptions = options.Any();
            var anyRequired = options.Any(o => o.OptionValueType == NDesk.Options.OptionValueType.Required);

            _requiredParametersSelection.SetActive(anyOptions && anyRequired);
            _nonRequiredParametersSelection.SetActive(anyOptions && !anyRequired);
        }

        private void Execute()
        {
            if (_requiredParametersSelection.activeSelf)
                InfoButtonClicked();
            else
                DebugConsole.ExecuteCommand($"{_command.Name}");
        }
    }
}