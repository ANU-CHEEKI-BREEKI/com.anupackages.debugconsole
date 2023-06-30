#if USE_NCALC
using System;
using System.Linq;
using ANU.IngameDebug.Console.CommandLinePreprocessors;
using ANU.IngameDebug.Utils;
using UnityEditor;
using UnityEngine;

[assembly: ANU.IngameDebug.Console.RegisterDebugCommandTypes(typeof(ANU.IngameDebug.Console.ExpressionEvaluatorPreprocessor))]

namespace ANU.IngameDebug.Console
{
    public class ExpressionEvaluatorPreprocessor : ICommandInputPreprocessor
    {
        int ICommandInputPreprocessor.Priority => 1_000;

        public string Preprocess(string input)
        {
            input = input.Trim();

            // skip evaluation for #define commands
            if (input.StartsWith("#"))
                return input;

            var cmdLine = input.SplitCommandLine();
            if (!cmdLine.Any())
                return input;

            var evaluatedArgs = cmdLine.Select(a =>
            {
                a = a.Trim();
                if (a.StartsWith("-"))
                {
                    var indOfEq = a.IndexOf("=");
                    if (indOfEq <= 0)
                        return a;

                    var option = a.Substring(0, indOfEq);
                    var value = a.Substring(indOfEq + 1).Trim('"').Trim('\'');
                    value = Evaluate(value, silent: true)?.ToString();

                    return @$"{option}=""{value}""";
                }
                else
                {
                    return Evaluate(a, silent: true);
                }
            });

            var result = string.Join(" ", evaluatedArgs);
            return result;
        }

        [DebugCommand(Name = "$", Description = @"Alias for evaluate command")]
        private static object EvaluateAlias(string expression) => Evaluate(expression);

        [DebugCommand(
            Name = "$evaluate",
            Description = @"Evaluate expression")]
        private static object Evaluate(
            [OptDesc(@"String expression. For example: ""5 * (2 + 3) / 2""")]
            string expression,
            [OptDesc(@"Do not print any Log messages")]
            bool silent = false
        )
        {
            try
            {
                var exp = new NCalc.Expression(
                    expression.ToLower(),
                    NCalc.EvaluateOptions.RoundAwayFromZero | NCalc.EvaluateOptions.NoCache | NCalc.EvaluateOptions.IgnoreCase
                );

                exp.Parameters["pi"] = Mathf.PI;
                exp.Parameters["deg2rad"] = Mathf.Deg2Rad;
                exp.Parameters["rad2deg"] = Mathf.Rad2Deg;

                exp.EvaluateFunction += (name, args) =>
                {
                    var parameters = args.EvaluateParameters().AsEnumerable().Select(i => Convert.ToSingle(i));

                    if (name == "clamp")
                        args.Result = Mathf.Clamp(parameters.ElementAt(0), parameters.ElementAt(1), parameters.ElementAt(2));
                    else if (name == "clamp01")
                        args.Result = Mathf.Clamp01(parameters.First());
                };

                var result = exp.Evaluate();
                return result;
            }
            catch (Exception ex)
            {
                if (!silent)
                    Debug.LogException(ex);
                return expression;
            }
        }
    }
}
#endif