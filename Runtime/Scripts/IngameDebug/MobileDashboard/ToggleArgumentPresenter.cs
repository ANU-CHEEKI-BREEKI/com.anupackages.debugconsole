using UnityEngine;
using UnityEngine.UI;
using static ANU.IngameDebug.Console.Commands.Implementations.MemberCommand;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal class ToggleArgumentPresenter : ArgumentPresenterBase
    {
        [SerializeField] private Toggle _toggle;

        protected override void PresentInternal(ParameterCache parameter)
        {
            throw new System.NotImplementedException();
        }
    }
}