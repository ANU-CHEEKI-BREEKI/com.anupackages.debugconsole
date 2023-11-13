using System;
using System.Collections.Generic;
using System.Linq;
using ANU.IngameDebug.Utils;

namespace ANU.IngameDebug.Console
{
    public abstract class ASuggestionContext<T> : ISuggestionsContext
    {
        public virtual IEnumerable<Suggestion> GetSuggestions(string input)
        {
            return FilterItems(
                Collection,
                input,
                GetFilteringName
            )
            .Select(c => new Suggestion(
                GetDisplayName(c),
                c,
                GetFullSuggestedText
            ));
        }

        protected abstract IEnumerable<T> Collection { get; }

        public abstract string Title { get; }

        protected abstract string GetDisplayName(T item);
        protected abstract string GetFilteringName(T item);
        protected abstract string GetFullSuggestedText(Suggestion item, string fullInput);

        private protected virtual IEnumerable<T> FilterItems(IEnumerable<T> items, string input, Func<T, string> filteredStringGetter)
        {
            return items
                .Select(c => new
                {
                    item = c,
                    str = filteredStringGetter.Invoke(c)
                })
                .Select(c => new
                {
                    item = c.item,
                    matches = c.str.FindMatches(input),
                    str = c.str
                })
                .Select(c =>
                {
                    var freeInput = input;
                    c.matches.ForEach(m => freeInput = freeInput.Replace(m.Value, ""));

                    var item = new
                    {
                        item = c.item,
                        matches = c.matches,
                        str = c.str,
                        freeInputChars = freeInput
                    };

                    return item;
                })
                .Where(c => (c.matches.Any() && c.freeInputChars.Length == 0) || c.str.ToLowerInvariant().Contains(input.ToLowerInvariant()))
                .OrderBy(c => c.matches.Count)
                .ThenBy(c => c.matches.Sum(s => s.Length))
                .Select(c => c.item);
        }
    }
}