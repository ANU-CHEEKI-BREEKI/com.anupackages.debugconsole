using ANU.IngameDebug.Console.Commands;
using ANU.IngameDebug.Console.Commands.Implementations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    [RequireComponent(typeof(LayoutElement))]
    internal abstract class CommandPresenterBase : MonoBehaviour
    {
        public struct InitArgs
        {
            public Sprite MethodIcon;
            public Sprite FieldIcon;
            public Sprite PropertyIcon;
        }

        [SerializeField] protected TextMeshProUGUI _label;
        [SerializeField] protected Button _info;
        [Space]
        [SerializeField] private Image _icon;

        private InitArgs _initArgs;

        private MemberCommand _command;

        protected void InfoButtonClicked()
        {
            //TODO: open popup with command name, description, arguments input
        }

        public virtual void Initialize(InitArgs initArgs)
        {
            _initArgs = initArgs;
            _info.onClick.AddListener(() => InfoButtonClicked());
        }

        public void Present(MemberCommand command)
        {
            _command = command;
            _label.text = command.Name;
            PresentInternal(command);

            _icon.sprite = command is FieldCommand
                ? _initArgs.FieldIcon
                : command is PropertyCommand
                    ? _initArgs.PropertyIcon
                    : _initArgs.MethodIcon;

            var layout = GetComponent<LayoutElement>();
            UpdateLayout(layout);
        }

        protected virtual void OnEnable()
        {
            // update prop and field values when enabled
            if (_command is FieldCommand || _command is PropertyCommand)
                PresentInternal(_command);
        }

        protected abstract void PresentInternal(MemberCommand command);

        protected virtual void UpdateLayout(LayoutElement layout)
        {
            var w = LayoutUtility.GetPreferredWidth(_label.rectTransform);
            w = Mathf.Clamp(w, 180, 650);
            if (w > _label.rectTransform.rect.width)
                w /= 2f;

            w = Mathf.Clamp(w, 180, 650) + 90;//90 - info button
            layout.minWidth = w;
        }
    }
}