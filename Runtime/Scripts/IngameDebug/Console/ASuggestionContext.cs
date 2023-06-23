using System;
using System.Collections.Generic;
using System.Linq;

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

        private protected virtual IOrderedEnumerable<T> FilterItems(IEnumerable<T> items, string input, Func<T, string> filteredStringGetter)
        {
            return items
                .Where(c => filteredStringGetter.Invoke(c).Contains(input))
                .OrderByDescending(c => filteredStringGetter.Invoke(c).IndexOf(input));
        }
    }
}