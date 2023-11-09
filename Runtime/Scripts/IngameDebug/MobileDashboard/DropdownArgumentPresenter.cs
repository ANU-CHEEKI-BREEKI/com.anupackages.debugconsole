using System.Linq;
using ANU.IngameDebug.Console.Commands.Implementations;
using TMPro;
using UnityEngine;
using static ANU.IngameDebug.Console.Commands.Implementations.MemberCommand;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal class DropdownArgumentPresenter : ArgumentPresenterBase
    {
        [SerializeField] private TMP_Dropdown _dropdown;

        public override string Value
        {
            get
            {
                try
                {
                    var values = Command.ValueHints[Parameter.Option].ToList();
                    return values.Skip(_dropdown.value).Take(1).Single();
                }
                catch
                {
                    return null;
                }
            }
        }

        protected override void Initialize() { }

        protected override void PresentInternal()
        {
            var values = Command.ValueHints[Parameter.Option].ToList();
            _dropdown.options.Clear();
            _dropdown.options.AddRange(values.Select(v => new TMP_Dropdown.OptionData(v)));

            var str = DebugConsole.Converters.ConvertToString(Parameter.DefaultValue);
            var index = values.IndexOf(str);
            _dropdown.SetValueWithoutNotify(index);
        }
    }
}