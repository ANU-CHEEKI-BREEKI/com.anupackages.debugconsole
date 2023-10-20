using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ANU.IngameDebug.Console.Commands.Implementations;
using ANU.IngameDebug.Utils;
using Codice.CM.Common;
using NCalc.Domain;
using UnityEngine;
using UnityEngine.Pool;

namespace ANU.IngameDebug.Console.Dashboard
{
    public class Dashboard : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private Transform _groupPrefab;
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

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);

            _content.DeleteAllChild();

            var commands = DebugConsole
                .Commands
                .Commands
                .Values
                .OfType<MemberCommand>()
                .Select(c => new
                {
                    Command = c,
                    Group = c.Name.Contains('.') ? c.Name.Substring(0, c.Name.LastIndexOf('.')) : "other",
                    ShortName = c.Name.Contains('.') ? c.Name.Substring(c.Name.LastIndexOf('.') + 1) : c.Name,
                })
                .OrderBy(c => c.Group)
                .ThenBy(c => c.ShortName)
                .GroupBy(c => c.Group);

            foreach (var group in commands)
            {
                var groupContent = GameObject.Instantiate(_groupPrefab, _content);

                foreach (var command in group)
                {
                    var item = command.Command;
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

                CommandPresenterBase Instantiate(CommandPresenterBase prefab) => GameObject.Instantiate(prefab, groupContent);
            }
        }

        private bool InRange(int value, int min, int max) => value >= min && value <= max;

    }
}