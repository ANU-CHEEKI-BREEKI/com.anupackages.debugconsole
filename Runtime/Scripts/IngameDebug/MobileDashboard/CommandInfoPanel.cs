using System.Collections.Generic;
using System.Linq;
using System.Text;
using ANU.IngameDebug.Console.Commands.Implementations;
using ANU.IngameDebug.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    [RequireComponent(typeof(Canvas))]
    internal class CommandInfoPanel : MonoBehaviour
    {
        [SerializeField] private Color _blockerColor;
        [Space]
        [SerializeField] private Button _hide;
        [SerializeField] private Button _execute;
        [SerializeField] private Button _info;
        [SerializeField] private Button _closeInfo;
        [Space]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _infoPanel;
        [Space]
        [SerializeField] private Transform _argumentsContent;
        [Space]
        [SerializeField] private DropdownArgumentPresenter _dropdown;
        [SerializeField] private SwitchArgumentPresenter _switch;
        [SerializeField] private ToggleArgumentPresenter _toggle;
        [SerializeField] private InputArgumentPresenter _input;
        [Space]
        [SerializeField] private TextMeshProUGUI _helpLabel;
        [SerializeField] private TextMeshProUGUI _nameLabel;

        private List<ArgumentPresenterBase> _presenters = new();

        private MemberCommand _command;
        private GameObject _blocker;

        private void Awake()
        {
            _hide.onClick.AddListener(Hide);
            _info.onClick.AddListener(() =>
            {
                _mainPanel.SetActive(false);
                _infoPanel.SetActive(true);
                _helpLabel.text = _command.GetHelp();
            });
            _closeInfo.onClick.AddListener(() =>
            {
                _mainPanel.SetActive(true);
                _infoPanel.SetActive(false);
            });
            _execute.onClick.AddListener(Execute);
            Hide();
        }

        public void Show(MemberCommand command)
        {
            Hide();

            _mainPanel.SetActive(true);
            _infoPanel.SetActive(false);

            var rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

            var dropdownCanvas = this.GetComponent<Canvas>();
            dropdownCanvas.overrideSorting = true;
            dropdownCanvas.sortingOrder = rootCanvas.sortingOrder + 100;
            dropdownCanvas.sortingLayerID = rootCanvas.sortingLayerID;

            _command = command;

            _blocker = CreateBlocker(rootCanvas);

            gameObject.SetActive(true);
            CreateArgumentPresenters();

            var name = _command.Name;
            var prefix = "";
            var dotIndex = name.LastIndexOf('.');
            if (dotIndex >= 0)
            {
                prefix = name.Substring(0, dotIndex + 1);
                name = name.Substring(dotIndex + 1);
            }
            _nameLabel.text = @$"<color=#AAA><size=60%>{prefix}</size></color>{name}";
        }

        public void Hide()
        {
            if (_blocker != null)
                Destroy(_blocker);

            gameObject.SetActive(false);
        }

        protected virtual GameObject CreateBlocker(Canvas rootCanvas)
        {
            var blocker = new GameObject("Blocker");

            var blockerRect = blocker.AddComponent<RectTransform>();
            blockerRect.SetParent(rootCanvas.transform, false);
            blockerRect.anchorMin = Vector3.zero;
            blockerRect.anchorMax = Vector3.one;
            blockerRect.sizeDelta = Vector2.zero;

            var blockerCanvas = blocker.AddComponent<Canvas>();
            blockerCanvas.overrideSorting = true;

            var dropdownCanvas = this.GetComponent<Canvas>();
            blockerCanvas.sortingLayerID = dropdownCanvas.sortingLayerID;
            blockerCanvas.sortingOrder = dropdownCanvas.sortingOrder - 1;

            Canvas parentCanvas = null;
            var parentTransform = transform.parent;
            while (parentTransform != null)
            {
                parentCanvas = parentTransform.GetComponent<Canvas>();
                if (parentCanvas != null)
                    break;

                parentTransform = parentTransform.parent;
            }

            if (parentCanvas != null)
            {
                var components = parentCanvas.GetComponents<BaseRaycaster>();
                for (int i = 0; i < components.Length; i++)
                {
                    var raycasterType = components[i].GetType();
                    if (blocker.GetComponent(raycasterType) == null)
                        blocker.AddComponent(raycasterType);
                }
            }
            else
            {
                GetOrAddComponent<GraphicRaycaster>(blocker);
            }

            var blockerImage = blocker.AddComponent<Image>();
            blockerImage.color = _blockerColor;

            var blockerButton = blocker.AddComponent<Button>();
            blockerButton.onClick.AddListener(Hide);

            return blocker;
        }

        private static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (!comp)
                comp = go.AddComponent<T>();
            return comp;
        }

        private void CreateArgumentPresenters()
        {
            _presenters.Clear();
            _argumentsContent.DeleteAllChild();
            foreach (var item in _command.ParametersCache)
            {
                var prefab = _input as ArgumentPresenterBase;
                if (item.Value.Type == typeof(bool))
                {
                    if (item.Value.IsRequired)
                        prefab = _toggle;
                    else
                        prefab = _switch;
                }
                else
                {
                    var hints = _command.ValueHints[item.Value.Option];
                    if (InRange(hints.Count(), 1, 4))
                        prefab = _switch;
                    else if (InRange(hints.Count(), 5, int.MaxValue))
                        prefab = _dropdown;
                }

                var presenter = Instantiate(prefab, _argumentsContent);
                presenter.Present(_command, item.Value);
                _presenters.Add(presenter);
            }
        }

        private void Execute()
        {
            var sb = new StringBuilder();
            sb.Append(_command.Name);
            sb.Append(" ");

            foreach (var item in _presenters)
            {
                var val = item.Value;
                if (val == null)
                    continue;

                sb.Append("--");
                sb.Append(item.Parameter.Option.GetNames()[0]);
                sb.Append("=");
                sb.Append("\"");
                sb.Append(item.Value);
                sb.Append("\" ");
            }

            DebugConsole.ExecuteCommand(sb.ToString());

            Hide();
        }

        private bool InRange(int value, int min, int max) => value >= min && value <= max;
    }
}