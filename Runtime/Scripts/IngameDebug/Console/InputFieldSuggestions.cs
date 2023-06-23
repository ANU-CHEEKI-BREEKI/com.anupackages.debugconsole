using System;
using System.Linq;
using ANU.IngameDebug.Utils;
using TMPro;
using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal class InputFieldSuggestions : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _input;
        [SerializeField] private TextMeshProUGUI _label;

        private void Awake() => _input.onValueChanged.AddListener(ShowSuggestions);

        private void ShowSuggestions(string text)
        {
            var names = text.SplitCommandLine();
            var commandName = names.FirstOrDefault();
            if (!DebugConsole.Commands.Commands.TryGetValue(commandName ?? "", out var command))
            {
                _label.text = "";
                return;
            }

            var namedParameter = text.GetFirstNamedParameter();
            if (namedParameter.Success)
            {
                _label.text = "";
                return;
            }

            var cnt = names.Skip(1).Count();
            var options = command.Options.Skip(cnt)
                .Select(o => new { name = o.GetNames().First(), option = o })
                .Select(o => o.option.OptionValueType == NDesk.Options.OptionValueType.Optional
                    ? $"[{o.name}]"
                    : o.option.OptionValueType == NDesk.Options.OptionValueType.None
                        ? $"-{o.option.GetNames().OrderBy(n => n.Length).First()}"
                        : o.name
                );
            _label.text = $"<alpha=#00>{text}<alpha=#FF> {string.Join(" ", options)}";
        }
    }
}