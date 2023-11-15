using ANU.IngameDebug.Console.Commands.Implementations;
using TMPro;
using UnityEngine;
using static ANU.IngameDebug.Console.Commands.Implementations.MemberCommand;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal abstract class ArgumentPresenterBase : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI _label;

        public ParameterCache Parameter { get; private set; }
        public MemberCommand Command { get; private set; }
        public abstract string Value { get; }

        private bool _isInitialized;

        public void Present(MemberCommand command, ParameterCache parameter)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Initialize();
            }

            Parameter = parameter;
            Command = command;
            _label.text = parameter.Name;
            PresentInternal();
        }

        protected virtual void OnEnable() { }
        protected abstract void Initialize();
        protected abstract void PresentInternal();
    }
}