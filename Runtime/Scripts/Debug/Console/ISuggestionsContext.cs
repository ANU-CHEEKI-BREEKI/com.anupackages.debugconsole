using System.Collections.Generic;

namespace ANU.IngameDebug.Console
{
    public interface ISuggestionsContext
    {
        string Title { get; }

        IEnumerable<Suggestion> GetSuggestions(string input);
    }
}