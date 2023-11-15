using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal class ToggleArgumentPresenter : ArgumentPresenterBase
    {
        [SerializeField] private Toggle _toggle;

        public override string Value => DebugConsole.Converters.ConvertToString(_toggle.isOn);

        protected override void Initialize() { }

        protected override void PresentInternal()
        {
            var initValue = false;
            try
            {
                initValue = (bool)Parameter.DefaultValue;
            }
            catch { }
            _toggle.SetIsOnWithoutNotify(initValue);
        }
    }
}