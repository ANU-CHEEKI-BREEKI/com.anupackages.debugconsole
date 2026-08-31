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
            // both sides lowered once - the match loops compare raw chars
            var search = input.ToLowerInvariant();

            return items
                .Select(c => new
                {
                    item = c,
                    str = filteredStringGetter.Invoke(c).ToLowerInvariant()
                })
                .Select(c => new
                {
                    c.item,
                    matches = c.str.FindMatches(search),
                    c.str
                })
                .Select(c => new
                {
                    c.item,
                    c.matches,
                    c.str,
                    // matches are sequential and non-overlapping, so the search string
                    // is fully covered exactly when the matched lengths sum up to it
                    covered = c.matches.Sum(m => m.Length)
                })
                .Where(c => (c.matches.Any() && c.covered == search.Length) || c.str.Contains(search))
                .OrderBy(c => c.matches.Count)
                .ThenBy(c => c.covered)
                .Select(c => c.item);
        }
    }
}