using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ANU.IngameDebug.Utils
{
    public static class Extensions
    {
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
    }
}