using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ANU.IngameDebug.Console
{
    internal class BracketsToStringPreprocessor : ICommandInputPreprocessor
    {
        private readonly Regex _regex;

        public BracketsToStringPreprocessor()
            => _regex = new Regex(@"(?<!""|')(?<content>\[.*?\])");

        public string Preprocess(string input)
        {
            var matches = _regex.Matches(input);
            input = _regex.Replace(input, @"""${content}""");
            // Debug.Log($"BracketsToStringPreprocessor: {input}");
            return input;
        }
    }
}