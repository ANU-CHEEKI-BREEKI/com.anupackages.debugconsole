using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ANU.IngameDebug.Console.Commands.Implementations.MemberCommand;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal class SwitchArgumentPresenter : ArgumentPresenterBase
    {
        [SerializeField] private ToggleGroup _group;
        [SerializeField] private List<Toggle> _toggles;

        protected override void PresentInternal(ParameterCache parameter)
        {
            throw new System.NotImplementedException();
        }
    }
}