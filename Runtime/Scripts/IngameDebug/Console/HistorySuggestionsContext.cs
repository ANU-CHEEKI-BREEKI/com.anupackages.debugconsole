using System.Collections.Generic;

namespace ANU.IngameDebug.Console
{
    public class HistorySuggestionsContext : ASuggestionContext<string>
    {
        private CommandLineHistory _commandsHistory;

        public HistorySuggestionsContext(CommandLineHistory commandsHistory)
        {
            _commandsHistory = commandsHistory;
        }

        public override string Title => "history";

        protected override IEnumerable<string> Collection => _commandsHistory.Commands;

        protected override string GetDisplayName(string item) => item;
        protected override string GetFilteringName(string item) => item;
        protected override string GetFullSuggestedText(Suggestion item, string fullInput) => item.Source as string + " ";
    }
}