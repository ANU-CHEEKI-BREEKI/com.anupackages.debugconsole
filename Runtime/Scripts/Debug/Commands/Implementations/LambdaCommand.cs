using System;
using System.Collections.Generic;
using NDesk.Options;

namespace IngameDebug.Commands.Implementations
{
    public class LambdaCommand : ADebugCommand
    {
        private Func<Dictionary<Option, AvailableValuesHint>, OptionSet> _optionsGetter;
        private readonly Action _onParced;

        public LambdaCommand(string name, string description, Func<Dictionary<Option, AvailableValuesHint>, OptionSet> optionsGetter, Action onParced = null) : base(name, description)
        {
            _optionsGetter = optionsGetter;
            _onParced = onParced;
        }

        public LambdaCommand(string name, string description, Action noOptions) : base(name, description)
            => _optionsGetter = valueHints => new OptionSet()
            {
                { "<>", v => noOptions?.Invoke() }
            };

        protected override OptionSet CreateOptions(Dictionary<Option, AvailableValuesHint> valueHints) => _optionsGetter.Invoke(InternalValueHints);
        protected override void OnParsed() => _onParced?.Invoke();
    }
}