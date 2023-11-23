using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace ANU.IngameDebug.Utils
{
    public static class Extensions
    {
        private static readonly Regex RegexFromCommandToFirstNamedParameter = new Regex(@"^(?<command>\s*[\.\w_\d\-]*).*?(?<named_parameter>\s+-{1,2}[\.\w_\d]*).*$");

        public static Group GetFirstNamedParameter(this string commandLine)
        {
            var match = RegexFromCommandToFirstNamedParameter.Match(commandLine);
            return match.Groups["named_parameter"];
        }

        public static IEnumerable<string> SplitCommandLine(this string commandLine)
        {
            var inQuotes = false;

            return commandLine
                .Split(c =>
                {
                    if (c == '\"')
                        inQuotes = !inQuotes;
                    return !inQuotes && c == ' ';
                })
                .Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
                .Where(arg => !string.IsNullOrEmpty(arg));
        }

        public static IEnumerable<string> Split(this string str, Func<char, bool> controller)
        {
            var nextPiece = 0;

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }

        public static string TrimMatchingQuotes(this string input, char quote)
        {
            if (input.Length >= 2
                && input[0] == quote
                && input[input.Length - 1] == quote)
                return input.Substring(1, input.Length - 2);

            return input;
        }

        public static int RemoveAll<T>(this LinkedList<T> list, Func<T, bool> predcate)
        {
            if (predcate == null || list.Count <= 0)
                return 0;

            var removed = 0;
            var next = list.First;
            while (next != null)
            {
                var current = next;
                next = next.Next;

                var value = current.Value;
                if (predcate.Invoke(value))
                {
                    list.Remove(current);
                    removed++;
                }
            }
            return removed;
        }

        private static Vector3[] _corners = new Vector3[4];
        public static Rect GetWorldRect(this RectTransform rt)
        {
            rt.GetWorldCorners(_corners);

            var min = new Vector2(
                _corners.Min(c => c.x),
                _corners.Min(c => c.y)
            );
            var max = new Vector2(
                _corners.Max(c => c.x),
                _corners.Max(c => c.y)
            );
            return new Rect(min, max - min);
        }

        public static bool TryGetIntersection(Rect firstRect, Rect secondRect, bool includeZeroSize, out Rect intersection)
        {
            intersection = new Rect()
            {
                xMin = Mathf.Max(firstRect.xMin, secondRect.xMin),
                yMin = Mathf.Max(firstRect.yMin, secondRect.yMin),
                xMax = Mathf.Min(firstRect.xMax, secondRect.xMax),
                yMax = Mathf.Min(firstRect.yMax, secondRect.yMax)
            };

            if (includeZeroSize)
                return intersection.size.x >= 0 && intersection.size.y >= 0;
            else
                return intersection.size.x > 0 && intersection.size.y > 0;
        }

        public static bool IsInside(this Rect innerRect, Rect outterRect, float epsilon = float.Epsilon)
        {
            if (!TryGetIntersection(outterRect, innerRect, includeZeroSize: false, out var intersection))
                return false;
            return Mathf.Abs(intersection.x - innerRect.x) <= epsilon
                && Mathf.Abs(intersection.y - innerRect.y) <= epsilon
                && Mathf.Abs(intersection.width - innerRect.width) <= epsilon
                && Mathf.Abs(intersection.height - innerRect.height) <= epsilon;
        }

        public static bool IsOutside(this Rect innerRect, Rect outterRect, float epsilon = float.Epsilon)
        {
            if (!TryGetIntersection(outterRect, innerRect, includeZeroSize: false, out var intersection))
                return true;

            return intersection.width < epsilon
                && intersection.height < epsilon;
        }

        public static string ToHexString(this Color color) => ColorUtility.ToHtmlStringRGBA(color);

        public static Coroutine InvokeSkipOneFrame(this MonoBehaviour monoBehaviour, Action method)
            => InvokeSkipFrames(monoBehaviour, method, 1);

        public static Coroutine InvokeSkipFrames(this MonoBehaviour monoBehaviour, Action method, int framesCount)
        {
            if (monoBehaviour == null || !monoBehaviour.gameObject.activeInHierarchy)
                return null;
            return monoBehaviour.StartCoroutine(InvokeCoroutineSkipFrames(method, framesCount));
        }

        private static IEnumerator InvokeCoroutineSkipFrames(Action method, int framesCount)
        {
            if (method == null)
                yield break;

            for (int i = 0; i < framesCount; i++)
                yield return null;
            method?.Invoke();
        }

        public enum ColumnAlignment
        {
            //
            // Summary:
            //     Text lines are aligned on the left side.
            Left = 0,
            //
            // Summary:
            //     Text lines are centered.
            Center = 1,
            //
            // Summary:
            //     Text lines are aligned on the right side.
            Right = 2
        }

        public static StringBuilder PrintTable<T>(this StringBuilder sb, IEnumerable<T> items, IEnumerable<string> getTitle, Func<T, IEnumerable<string>> getRow, IEnumerable<ColumnAlignment> alignment = null, IEnumerable<int> extraSpaceLeft = null, IEnumerable<int> extraSpaceRight = null)
        {
            if (sb is null)
                throw new ArgumentNullException(nameof(sb));

            if (items is null)
                throw new ArgumentNullException(nameof(items));

            if (getTitle is null)
                throw new ArgumentNullException(nameof(getTitle));

            if (getRow is null)
                throw new ArgumentNullException(nameof(getRow));

            var title = getTitle;
            var rows = items.Select(i => getRow(i));

            var titleLength = title
                .Select((t, i) => rows
                    .Select(r => r.ElementAt(i)).Prepend(t)
                    .Max(l => l.Length)
                )
                .ToArray();

            PrintRow(title);

            sb.AppendLine();

            var sum = titleLength.Sum() + titleLength.Count();
            var esl = extraSpaceLeft?.Sum() ?? (titleLength.Count() * 2);
            var esr = extraSpaceRight?.Sum() ?? (titleLength.Count() * 2);
            sum += esl + esr;

            for (int i = 0; i < sum; i++)
                sb.Append("_");

            foreach (var row in rows)
            {
                sb.AppendLine();

                PrintRow(row);
            }

            return sb;

            void PrintRow(IEnumerable<string> items)
            {
                var tLen = items.Zip(titleLength.Select((t, i) => new { length = t, index = i }), (t, l) => new { title = t, length = l });
                foreach (var item in tLen)
                {
                    var t = item.title;
                    var l = item.length.length;
                    var index = item.length.index;

                    var len = l - t.Length;
                    var al = alignment?.ElementAtOrDefault(index) ?? ColumnAlignment.Left;

                    var esl = extraSpaceLeft?.ElementAtOrDefault(index) ?? 2;
                    var esr = extraSpaceRight?.ElementAtOrDefault(index) ?? 2;

                    for (int i = 0; i < esl; i++)
                        sb.Append(" ");

                    if (al == ColumnAlignment.Right)
                        for (int i = 0; i < len; i++)
                            sb.Append(" ");

                    if (al == ColumnAlignment.Center)
                    {
                        var l2 = len / 2;
                        if (len % 2 != 0)
                            l2++;

                        for (int i = 0; i < l2; i++)
                            sb.Append(" ");
                    }

                    sb.Append(item.title);

                    if (al == ColumnAlignment.Left)
                        for (int i = 0; i < len; i++)
                            sb.Append(" ");

                    if (al == ColumnAlignment.Center)
                        for (int i = 0; i < len / 2; i++)
                            sb.Append(" ");

                    for (int i = 0; i < esr; i++)
                        sb.Append(" ");

                    sb.Append("|");
                }
            }
        }

        public static void DeleteAllChild(this Component component)
        {
            var tr = component.transform;
            while (tr.childCount > 0)
            {
                var c = tr.GetChild(0);
                c.SetParent(null);
                GameObject.Destroy(c.gameObject);
            }
        }

        public static Coroutine TweenFloat(this MonoBehaviour mono, float from, float to, float duration, Action<float> callback)
            => mono.StartCoroutine(TweenFloatCoroutine(from, to, duration, callback));

        public static Coroutine TweenAnimation(this MonoBehaviour mono, Action<float> callback)
            => mono.TweenFloat(0, 1, 0.1f, callback);

        private static IEnumerator TweenFloatCoroutine(float from, float to, float duration, Action<float> callback)
        {
            var timer = 0f;

            while (timer < duration)
            {
                var t = timer / duration;
                callback?.Invoke(
                    Mathf.Lerp(from, to, t)
                );
                yield return null;
                timer += Time.unscaledDeltaTime;
            }

            callback?.Invoke(to);
        }
    
        public static TMP_InputField.ContentType GetContentType(this object value)
            => value switch
            {
                short => TMP_InputField.ContentType.IntegerNumber,
                int => TMP_InputField.ContentType.IntegerNumber,
                long => TMP_InputField.ContentType.IntegerNumber,
                ushort => TMP_InputField.ContentType.IntegerNumber,
                uint => TMP_InputField.ContentType.IntegerNumber,
                ulong => TMP_InputField.ContentType.IntegerNumber,
                sbyte => TMP_InputField.ContentType.IntegerNumber,
                byte => TMP_InputField.ContentType.IntegerNumber,

                float => TMP_InputField.ContentType.DecimalNumber,
                double => TMP_InputField.ContentType.DecimalNumber,
                decimal => TMP_InputField.ContentType.DecimalNumber,

                _ => TMP_InputField.ContentType.Standard,
            };
    }
}