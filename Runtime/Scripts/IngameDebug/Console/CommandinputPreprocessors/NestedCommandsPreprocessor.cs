using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using ANU.IngameDebug.Console.CommandLinePreprocessors;

namespace ANU.IngameDebug.Console
{
    public class NestedCommandsPreprocessor : ICommandInputPreprocessor, IInjectDebugConsoleContext
    {
        private static readonly Regex _expressionRegex = new(@"\{(?!.*?\{)(?<command>.*?)(?<!.*?\})\}");

        private bool _nestedCall;

        public IReadOnlyDebugConsoleProcessor Context { get; set; }
        int ICommandInputPreprocessor.Priority => -100;

        public string Preprocess(string input)
        {
            if (_nestedCall)
                return input;

            input = input.Trim();

            //FIXME: somehow move this from all preprocessors. 
            // need to allow single preprocessor to discard other preprocessors

            // skip evaluation for #define commands
            if (input.StartsWith("#"))
                return input;

            if (!_expressionRegex.IsMatch(input))
                return input;

            using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
            {
                _nestedCall = true;

                //WE can not use thread, because commands can access UnityEngine code
                // which can be accessible only from work thread
                try
                {
                    while (_expressionRegex.IsMatch(input) && !tokenSource.IsCancellationRequested)
                    {
                        input = _expressionRegex.Replace(
                            input,
                            m =>
                            {
                                var nestedCommand = m.Groups["command"].Value;
                                var nestedResult = Context.ExecuteCommand(nestedCommand, silent: true);
                                var val = nestedResult.ReturnValues.LastOrDefault().ReturnValue;
                                return Context.Converters.ConvertToString(val);
                            }
                        );
                    }
                }
                catch (Exception ex)
                {
                    Context.Logger.LogException(ex);
                }

                _nestedCall = false;

                if (tokenSource.IsCancellationRequested)
                    Context.Logger.LogWarning($"Nested commands evaluation timeout hitted. End result are invalid: {input}");
            }

            return input;
        }
    }
}