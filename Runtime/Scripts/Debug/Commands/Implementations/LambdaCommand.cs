using System;
using NDesk.Options;

namespace IngameDebug.Commands.Implementations
{
    public class LambdaCommand : ADebugCommand
    {
        private Func<OptionSet> _optionsGetter;
        private readonly Action _onParced;

        public LambdaCommand(string name, string description, Func<OptionSet> optionsGetter, Action onParced = null) : base(name, description)
        {
            _optionsGetter = optionsGetter;
            _onParced = onParced;
        }

        public LambdaCommand(string name, string description, Action noOptions) : base(name, description)
            => _optionsGetter = () => new OptionSet()
            {
                { "<>", v => noOptions?.Invoke() }
            };

        protected override OptionSet CreateOptions() => _optionsGetter.Invoke();
        protected override void OnParsed() => _onParced?.Invoke();
    }
}