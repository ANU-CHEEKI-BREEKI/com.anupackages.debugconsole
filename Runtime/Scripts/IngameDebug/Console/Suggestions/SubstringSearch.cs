using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// [assembly: ANU.IngameDebug.Console.RegisterDebugCommandTypes(typeof(ANU.IngameDebug.Utils.SubstringSearch))]

namespace ANU.IngameDebug.Utils
{
    public static class SubstringSearch
    {
        // greedy: repeatedly takes the longest common run (2+ chars) between the
        // still-unused parts of both strings, so search terms match in any order.
        // case-sensitive on purpose: FilterItems pre-lowers both strings once,
        // per-char ToLowerInvariant here multiplies over the O(n*m) loops
        public static List<Match> FindMatches(this string input, string search)
        {
            var matches = new List<Match>();
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(search))
                return matches;

            var inputUsed = new bool[input.Length];
            var searchUsed = new bool[search.Length];

            while (true)
            {
                var best = FindLongestCommonRun(input, inputUsed, search, searchUsed);
                if (best.Length < 2)
                    break;

                matches.Add(best);

                for (int index = 0; index < best.Length; index++)
                {
                    inputUsed[best.InputIndex + index] = true;
                    searchUsed[best.SearchIndex + index] = true;
                }
            }

            return matches;
        }

        private static Match FindLongestCommonRun(string input, bool[] inputUsed, string search, bool[] searchUsed)
        {
            var best = new Match(input, search);

            for (int s = 0; s < search.Length; s++)
            {
                if (searchUsed[s])
                    continue;

                for (int i = 0; i < input.Length; i++)
                {
                    if (inputUsed[i] || input[i] != search[s])
                        continue;

                    var length = 1;
                    while (s + length < search.Length
                        && i + length < input.Length
                        && !searchUsed[s + length]
                        && !inputUsed[i + length]
                        && input[i + length] == search[s + length])
                        length++;

                    if (length > best.Length)
                    {
                        best.Success = true;
                        best.InputIndex = i;
                        best.SearchIndex = s;
                        best.Length = length;
                    }
                }
            }

            return best;
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