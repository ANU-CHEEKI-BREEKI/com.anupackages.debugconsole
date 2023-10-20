using System.Collections;
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

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(isOn =>
            {
                _layout.enabled = isOn;
                foreach (var item in _layout.RectChildren)
                    item.gameObject.SetActive(isOn);
            });
        }

        public void Initialize(string title, IEnumerable<MemberCommand> commands)
        {
            _title.text = title;
            
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
            }

        }

        private CommandPresenterBase Instantiate(CommandPresenterBase prefab) => GameObject.Instantiate(prefab, transform);
        private bool InRange(int value, int min, int max) => value >= min && value <= max;
    }
}