#if USE_NCALC
using System;
using System.Linq;
using System.Text.RegularExpressions;
using ANU.IngameDebug.Console.CommandLinePreprocessors;
using ANU.IngameDebug.Utils;
using UnityEditor;
using UnityEngine;

[assembly: ANU.IngameDebug.Console.RegisterDebugCommandTypes(typeof(ANU.IngameDebug.Console.ExpressionEvaluatorPreprocessor))]

namespace ANU.IngameDebug.Console
{
    public class ExpressionEvaluatorPreprocessor : ICommandInputPreprocessor
    {
        private static readonly Regex _expressionRegex = new(@"\${(?<expression>.*?)}");

        int ICommandInputPreprocessor.Priority => 1_000;

        public string Preprocess(string input)
        {
            var args = input.SplitCommandLine();
            var name = args.FirstOrDefault();
            args = args.Skip(1).Select(a =>
            {
                if (a.StartsWith("-"))
                {
                    var values = a.Split('=', System.StringSplitOptions.RemoveEmptyEntries);
                    if (values.Length <= 1)
                        return EvaluateIfMathes(a);

                    var opt = values.FirstOrDefault();
                    var val = values.LastOrDefault();

                    return $"{opt}={EvaluateIfMathes(val)}";
                }
                else
                {
                    return EvaluateIfMathes(a);
                }
            });

            return string.Join(" ", args.Prepend(name));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RegisterSelf() => DebugConsole.Preprocessors.Add(new ExpressionEvaluatorPreprocessor());

        private static string EvaluateIfMathes(string input)
        {
            var match = _expressionRegex.Match(input);
            if (match.Success)
                return Evaluate(match.Groups["expression"].Value);
            else
                return input;
        }

        [DebugCommand(Name = "$", Description = @"Alias for Evaluate command
You can call this command as nested command to pass expression as parameter. 
Use: ""${expression}""
For example: ""echo ${1+4}""")]
        private static string EvaluateAlias(string expression) => Evaluate(expression);

        [DebugCommand(Description = @"Evaluate expression
You can call this command as nested command to pass expression as parameter. 
Use short syntax for this command: ""${expression}""
For example: ""echo ${1+4}""")]
        private static string Evaluate(
            [OptDesc(@"String expression. For example: ""5 * (2 + 3) / 2""")]
            string expression
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
                return result?.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return expression;
            }
        }
    }
}
#endif