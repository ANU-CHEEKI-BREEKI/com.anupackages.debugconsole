using System;
using System.Collections.Generic;
using System.Linq;
using ANU.IngameDebug.Console.Commands.Implementations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ANU.IngameDebug.Console.Dashboard
{
    public class DashboardGroup : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private DashboardLayout _layout;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private GameObject _header;
        [SerializeField] private LayoutElement _element;
        [Space]
        [SerializeField] private GenericCommandPresenter _genericPresenterPrefab;
        [SerializeField] private ToggleCommandPresenter _togglePresenterPrefab;
        [SerializeField] private SwitchCommandPresenter _switchCommandPresenter;
        [SerializeField] private DropdownCommandPresenter _dropdownCommandPresenter;
        [SerializeField] private InputCommandPresenter _inputCommandPresenter;
        [Space]
        [SerializeField] private Sprite _methodIcon;
        [SerializeField] private Sprite _fieldIcon;
        [SerializeField] private Sprite _propertyIcon;

        public event Action<MemberCommand> InfoRequested;

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(isOn =>
            {
                _layout.enabled = isOn;
                foreach (var item in _layout.RectChildren)
                    item.gameObject.SetActive(isOn);
            });
        }

        public void Initialize(string title, IEnumerable<MemberCommand> commands, bool showHeader)
        {
            _header.SetActive(showHeader);
            _element.minHeight = showHeader ? 80 : 5;
            _layout.padding.top = showHeader ? 95 : 15;

            _title.text = title;

            DebugConsole.Processor._inputLogger.SilenceStack.Push(true);
            DebugConsole.Processor._logger.SilenceStack.Push(true);
            try
            {
                foreach (var command in commands)
                {
                    var item = command;
                    CommandPresenterBase presenter = null;

                    if (item.ParametersCache.Count == 1)
                    {
                        if ((item is FieldCommand || item is PropertyCommand)
                            && item.ParametersCache[0].Type == typeof(bool))
                            presenter = Instantiate(_togglePresenterPrefab);
                        else if (item.ValueHints.Count > 0
                            && InRange(item.ValueHints.First().Value.Count(), 1, 4)
                            && (item is not MethodCommand || item.ParametersCache[0].IsRequired))
                            presenter = Instantiate(_switchCommandPresenter);
                        else if (item.ValueHints.Count > 0
                            && InRange(item.ValueHints.First().Value.Count(), 5, int.MaxValue)
                            && (item is not MethodCommand || item.ParametersCache[0].IsRequired))
                            presenter = Instantiate(_dropdownCommandPresenter);
                        else if (item.ParametersCache[0].Type != typeof(bool))
                            presenter = Instantiate(_inputCommandPresenter);
                    }

                    if (presenter == null)
                        presenter = Instantiate(_genericPresenterPrefab);

                    // presenter.transform.SetParent(_content, false);
                    presenter.Initialize(new CommandPresenterBase.InitArgs
                    {
                        MethodIcon = _methodIcon,
                        PropertyIcon = _propertyIcon,
                        FieldIcon = _fieldIcon
                    });
                    presenter.Present(item);

                    presenter.InfoRequested += presenter => InfoRequested?.Invoke(presenter.Command);
                }
            }
            finally
            {
                DebugConsole.Processor._inputLogger.SilenceStack.TryPop(out _);
                DebugConsole.Processor._logger.SilenceStack.TryPop(out _);
            }
        }

        private CommandPresenterBase Instantiate(CommandPresenterBase prefab) => GameObject.Instantiate(prefab, transform);
        private bool InRange(int value, int min, int max) => value >= min && value <= max;
    }
}