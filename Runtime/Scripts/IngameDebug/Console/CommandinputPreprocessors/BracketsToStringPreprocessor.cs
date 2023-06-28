using System;
using System.Text.RegularExpressions;
using ANU.IngameDebug.Console.CommandLinePreprocessors;
using UnityEngine;

namespace ANU.IngameDebug.Console
{
    public class BracketsToStringPreprocessor : ICommandInputPreprocessor
    {
        private readonly Regex _regex = new Regex(@"(?<!""|')\s+(?<content>(\[.*?\])|(\(.*?\)))");

        public string Preprocess(string input)
        {
            var matches = _regex.Matches(input);
            input = _regex.Replace(input, @" ""${content}""");
            return input;
        }
    }
}