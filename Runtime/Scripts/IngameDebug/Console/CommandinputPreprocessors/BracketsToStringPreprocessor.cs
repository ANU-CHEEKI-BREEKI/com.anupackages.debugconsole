using System.Text;
using ANU.IngameDebug.Console.CommandLinePreprocessors;

namespace ANU.IngameDebug.Console
{
    public class BracketsToStringPreprocessor : ICommandInputPreprocessor
    {
        // wraps every top-level (...) or [...] argument in quotes so it survives
        // command line splitting. a scanner instead of a regex: lazy matching cut
        // nested groups like ((0.2, 5) (35.4, 29)) at the first closing bracket
        public string Preprocess(string input)
        {
            var sb = new StringBuilder(input.Length + 8);
            var inQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];

                if (c == '"')
                    inQuotes = !inQuotes;

                var opensGroup = !inQuotes
                    && (c == '(' || c == '[')
                    && i > 0
                    && char.IsWhiteSpace(input[i - 1]);

                var end = opensGroup ? FindBalancedEnd(input, i) : -1;

                if (end < 0)
                {
                    sb.Append(c);
                    continue;
                }

                sb.Append('"').Append(input, i, end - i + 1).Append('"');
                i = end;
            }

            return sb.ToString();
        }

        private static int FindBalancedEnd(string input, int start)
        {
            var open = input[start];
            var close = open == '(' ? ')' : ']';
            var depth = 0;

            for (int i = start; i < input.Length; i++)
            {
                if (input[i] == open)
                {
                    depth++;
                }
                else if (input[i] == close)
                {
                    depth--;
                    if (depth == 0)
                        return i;
                }
            }

            return -1;
        }
    }
}
