using System;
using System.Text.RegularExpressions;
using ANU.IngameDebug.Console.CommandLinePreprocessors;
using UnityEngine;

namespace ANU.IngameDebug.Console
{
    public class BracketsToStringPreprocessor : ICommandInputPreprocessor
    {
        private readonly Regex _regex;

        public BracketsToStringPreprocessor()
            => _regex = new Regex(@"(?<!""|')(?<content>(\[.*?\])|(\(.*?\)))");

        public string Preprocess(string input)
        {
            var matches = _regex.Matches(input);
            input = _regex.Replace(input, @"""${content}""");
            return input;
        }
    }
}