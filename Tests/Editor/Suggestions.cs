using System.Linq;
using ANU.IngameDebug.Console.Commands;
using ANU.IngameDebug.Console.Dashboard;
using NUnit.Framework;

namespace ANU.IngameDebug.Console.Editor.Tests
{
    public class Suggestions : TestBase
    {
        private enum Blade
        {
            FalcataIron,
            FalcataBronze,
            GladiusCopper
        }

        private CommandsSuggestionsContext _suggestions;

        [OneTimeSetUp]
        public override void SetUpOnce()
        {
            base.SetUpOnce();

            Context.Commands.RegisterCommand<Blade>("suggestions-test-drop", "test command", blade => { });
            Context.Commands.RegisterCommand<Blade, Blade>("suggestions-test-two", "test command", (first, second) => { });

            _suggestions = new CommandsSuggestionsContext(Context.Commands.Commands);
        }

        private string[] ValueSources(string input)
            => _suggestions
                .GetSuggestions(input)
                .Select(s => s.Source.ToString())
                .ToArray();

        private string ApplyFirst(string input)
            => _suggestions
                .GetSuggestions(input)
                .First()
                .ApplySuggestion(input);

        [Test]
        public void CommandName_FoundByMushedLowercaseTerms()
        {
            var suggestions = _suggestions.GetSuggestions("sugtestdrop");

            Assert.That(
                suggestions.Select(s => (s.Source as ADebugCommand)?.Name),
                Does.Contain("suggestions-test-drop")
            );
        }

        [Test]
        public void CommandName_FoundIgnoringCase()
        {
            var suggestions = _suggestions.GetSuggestions("SugTestDrop");

            Assert.That(
                suggestions.Select(s => (s.Source as ADebugCommand)?.Name),
                Does.Contain("suggestions-test-drop")
            );
        }

        [Test]
        public void ParameterValue_FilteredByPartialValue()
            => Assert.That(
                ValueSources("suggestions-test-drop falca"),
                Is.EquivalentTo(new[] { nameof(Blade.FalcataIron), nameof(Blade.FalcataBronze) })
            );

        [Test]
        public void ParameterValue_FoundByMushedLowercaseTerms()
            => Assert.That(
                ValueSources("suggestions-test-drop falcairon"),
                Is.EquivalentTo(new[] { nameof(Blade.FalcataIron) })
            );

        [Test]
        public void ParameterValue_EmptyPartialListsAllHints()
            => Assert.That(
                ValueSources("suggestions-test-drop "),
                Is.EquivalentTo(new[] { nameof(Blade.FalcataIron), nameof(Blade.FalcataBronze), nameof(Blade.GladiusCopper) })
            );

        [Test]
        public void ParameterValue_TermsMatchInAnyOrder()
            => Assert.That(
                ValueSources("suggestions-test-drop bronfalca"),
                Is.EquivalentTo(new[] { nameof(Blade.FalcataBronze) })
            );

        [Test]
        public void CommandName_TermsMatchInAnyOrder()
        {
            var suggestions = _suggestions.GetSuggestions("dropsugtest");

            Assert.That(
                suggestions.Select(s => (s.Source as ADebugCommand)?.Name),
                Does.Contain("suggestions-test-drop")
            );
        }

        [Test]
        public void Apply_ReplacesPartialPositionalValue()
            => Assert.That(
                ApplyFirst("suggestions-test-drop falcairon"),
                Is.EqualTo("suggestions-test-drop FalcataIron ")
            );

        [Test]
        public void Apply_KeepsNamedParameterPrefix()
            => Assert.That(
                ApplyFirst("suggestions-test-drop --blade=falcairon"),
                Is.EqualTo("suggestions-test-drop --blade=FalcataIron ")
            );

        [Test]
        public void Apply_IgnoresEqualsSignInEarlierArguments()
            => Assert.That(
                ApplyFirst("suggestions-test-two --first=GladiusCopper --second falcairon"),
                Is.EqualTo("suggestions-test-two --first=GladiusCopper --second FalcataIron ")
            );

        [Test]
        public void DevicePlatform_InEditorIsEditorOnly()
        {
            Assert.That(ConsolePlatformExtensions.GetCurrentDevicePlatform(), Is.EqualTo(TargetPlatforms.Editor));
            Assert.That(TargetPlatforms.Editor.HasCurrentDevicePlatform(), Is.True);
            Assert.That(TargetPlatforms.Any.HasCurrentDevicePlatform(), Is.True);
            Assert.That((TargetPlatforms.Mobile | TargetPlatforms.PC | TargetPlatforms.WebGL).HasCurrentDevicePlatform(), Is.False);
        }

        [Test]
        public void PermissivePlatform_InEditorMatchesEditor()
            => Assert.That(TargetPlatforms.Editor.HasCurrentPlatform(), Is.True);
    }
}
