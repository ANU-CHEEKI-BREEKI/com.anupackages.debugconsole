using ANU.IngameDebug.Console.Commands;
using ANU.IngameDebug.Console.Commands.Implementations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal abstract class CommandPresenterBase : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Button _info;

        private MemberCommand _command;

        protected virtual void Awake()
        {
            _info.onClick.AddListener(() => InfoButtonClicked());
        }

        protected void InfoButtonClicked()
        {
            //TODO: open popup with command name, description, arguments input
        }

        public void Present(MemberCommand command)
        {
            _command = command;
            _label.text = command.Name;
            PresentInternal(command);
        }

        private void OnEnable()
        {
            // update prop and field values when enabled
            if (_command is FieldCommand || _command is PropertyCommand)
                PresentInternal(_command);
        }

        protected abstract void PresentInternal(MemberCommand command);
    }
}