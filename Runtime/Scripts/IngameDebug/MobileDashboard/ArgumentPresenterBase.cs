using ANU.IngameDebug.Console.Commands.Implementations;
using TMPro;
using UnityEngine;
using static ANU.IngameDebug.Console.Commands.Implementations.MemberCommand;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal abstract class ArgumentPresenterBase : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI _label;

        public ParameterCache _parameter;

        public void Present(ParameterCache parameter)
        {
            _parameter = parameter;
            _label.text = parameter.Name;
            PresentInternal(_parameter);
        }
        protected virtual void OnEnable() { }
        protected abstract void PresentInternal(ParameterCache parameter);
    }
}