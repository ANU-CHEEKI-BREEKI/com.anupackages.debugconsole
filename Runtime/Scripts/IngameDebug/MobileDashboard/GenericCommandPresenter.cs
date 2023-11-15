using System.Linq;
using ANU.IngameDebug.Console.Commands.Implementations;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal class GenericCommandPresenter : CommandPresenterBase
    {
        [SerializeField] private GameObject _requiredParametersSelection;
        [SerializeField] private GameObject _nonRequiredParametersSelection;
        [SerializeField] private Button _button;

        private MemberCommand _command;

        public override void Initialize(InitArgs initArgs)
        {
            base.Initialize(initArgs);
            
            _button.onClick.AddListener(Execute);
        }

        protected override void PresentInternal(MemberCommand command)
        {
            _command = command;

            var anyOptions = command.ParametersCache.Count > 0;
            var anyRequired = command.ParametersCache.Values.Any(v => v.IsRequired);

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