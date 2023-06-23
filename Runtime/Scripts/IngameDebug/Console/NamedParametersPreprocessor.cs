using System.Linq;
using System.Text.RegularExpressions;
using ANU.IngameDebug.Console.CommandLinePreprocessors;
using ANU.IngameDebug.Utils;
using UnityEngine;

namespace ANU.IngameDebug.Console
{
    public class NamedParametersPreprocessor : ICommandInputPreprocessor
    {
        public int Priority => 100;

        public string Preprocess(string input, PreprocessorExtraArgs args)
        {
            var namedParameterGroup = input.GetFirstNamedParameter();

            var inputToAddParameterNames = input;
            var concatInput = "";

            if (namedParameterGroup.Success)
            {
                inputToAddParameterNames = input.Substring(0, namedParameterGroup.Index);
                concatInput = input.Substring(namedParameterGroup.Index);
            }

            var commandLine = inputToAddParameterNames.SplitCommandLine();

            var commandName = commandLine.First();

            if (!DebugConsole.Commands.Commands.TryGetValue(commandName, out var command))
            {
                // Debug.Log($"There are no command: {commandName}");
                return input;
            }

            var namedParameters = commandLine.Skip(1).Zip(command.Options, (p, o) =>
            {
                return $"--{o.GetNames().First()}=\"{p}\"";
            });

            input = string.Join(" ", namedParameters.Prepend(commandName).Append(concatInput));
            // Debug.Log($"NamedParametersPreprocessor: {input}");

            return input;
        }
    }
}