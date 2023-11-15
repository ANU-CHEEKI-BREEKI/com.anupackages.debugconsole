using ANU.IngameDebug.Utils;
using TMPro;
using UnityEngine;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal class InputArgumentPresenter : ArgumentPresenterBase
    {
        [SerializeField] private TMP_InputField _input;

        public override string Value => string.IsNullOrEmpty(_input.text) ? null : _input.text;

        protected override void Initialize() { }

        protected override void PresentInternal()
        {
            _input.SetTextWithoutNotify(
                DebugConsole.Converters.ConvertToString(Parameter.DefaultValue)
            );
            _input.contentType = Parameter.DefaultValue.GetContentType();
        }
    }
}