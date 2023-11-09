using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ANU.IngameDebug.Console.Commands.Implementations.MemberCommand;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal class InputArgumentPresenter : ArgumentPresenterBase
    {
        [SerializeField] private TMP_InputField _input;
        [SerializeField] private Button _button;

        protected override void PresentInternal(ParameterCache parameter)
        {
            throw new System.NotImplementedException();
        }
    }
}