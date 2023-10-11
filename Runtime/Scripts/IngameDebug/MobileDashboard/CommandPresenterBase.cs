using ANU.IngameDebug.Console.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    internal abstract class CommandPresenterBase : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Button _info;

        protected virtual void Awake()
        {
            _info.onClick.AddListener(() => InfoButtonClicked());
        }

        protected void InfoButtonClicked()
        {
            //TODO: open popup with command name, description, arguments input
        }

        public void Present(ADebugCommand command)
        {
            _label.text = command.Name;
            PresentInternal(command);
        }

        protected abstract void PresentInternal(ADebugCommand command);
    }
}