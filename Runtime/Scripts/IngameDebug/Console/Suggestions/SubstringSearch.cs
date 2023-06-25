using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// [assembly: ANU.IngameDebug.Console.RegisterDebugCommandTypes(typeof(ANU.IngameDebug.Utils.SubstringSearch))]

namespace ANU.IngameDebug.Utils
{
    public static class SubstringSearch
    {
        public static List<Match> FindMatches(this string input, string search)
        {
            Match lastMatch = default;
            var matches = new List<Match>();

            do
            {
                var match = Find(input, search, lastMatch);

                if (match.Success && match.Length > 1)
                    matches.Add(match);

                if (match.Length == 1)
                    match.SearchIndex--;

                lastMatch = match;
            }
            while (lastMatch.Success);

            return matches;
        }

        private static Match Find(string input, string search, Match lastMatch)
        {
            var match = new Match(input, search);

            for (int s = lastMatch.SearchEnd; s < search.Length; s++)
            {
                var b = search[s];

                for (int i = lastMatch.InputEnd; i < input.Length; i++)
                {
                    var a = input[i];

                    if (a == b)
                    {
                        match.Success = true;
                        match.InputIndex = i;
                        match.SearchIndex = s;
                        break;
                    }
                }

                if (match.Success)
                    break;
            }

            if (!match.Success)
                return match;

            var iStart = match.InputIndex;
            var sStart = match.SearchIndex;

            var len = Mathf.Min(
                input.Length - iStart,
                search.Length - sStart
            );

            for (int index = 0; index < len; index++)
            {
                var i = index + iStart;
                var s = index + sStart;

                var a = input[i];
                var b = search[s];

                if (a != b)
                    break;
                else
                    match.Length++;
            }

            return match;
        }

        [IngameDebug.Console.DebugCommand]
        private static async void TestSearch(string input, string search)
        {
            List<Match> matches = null;
            var t = new System.Threading.Thread(() =>
            {
                matches = FindMatches(input, search);

            });
            t.Start();

            var tokenSource = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(5));
            while (!tokenSource.Token.IsCancellationRequested && t.IsAlive)
                await System.Threading.Tasks.Task.Yield();

            if (t.IsAlive)
            {
                t.Abort();
                Debug.LogWarning("thread aborted");
            }

            Debug.Log($"matches: {string.Join(", ", matches?.Select(m => m.Value))}");
        }
    }

    public struct Match
    {
        public readonly string Input;
        public readonly string Search;

        public Match(string input, string search) : this()
        {
            Input = input;
            Search = search;
        }

        public bool Success { get; internal set; }

        internal int InputIndex { get; set; }
        internal int SearchIndex { get; set; }

        internal int Length { get; set; }

        internal int InputEnd => InputIndex + Length;
        internal int SearchEnd => SearchIndex + Length;

        public string Value => Input.Substring(InputIndex, Length);
    }
}