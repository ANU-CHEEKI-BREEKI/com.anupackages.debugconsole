using TMPro;
using UnityEngine;
using static ANU.IngameDebug.Console.Commands.Implementations.MemberCommand;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal class DropdownArgumentPresenter : ArgumentPresenterBase
    {
        [SerializeField] private TMP_Dropdown _dropdown;

        protected override void PresentInternal(ParameterCache parameter)
        {
            throw new System.NotImplementedException();
        }
    }
}