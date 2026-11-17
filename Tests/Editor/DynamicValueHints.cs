using System.Linq;
using ANU.IngameDebug.Console.Commands;
using NUnit.Framework;

namespace ANU.IngameDebug.Console.Editor.Tests
{
    public class DynamicValueHints : TestBase
    {
        private const string Provider = "dynamic-hints-test.list-values";

        private enum Blade
        {
            FalcataIron,
            GladiusCopper
        }

        private static void PickDynamic([OptValDynamic(Provider)] string value) { }
        private static void PickBlade(Blade blade) { }

        [Test]
        public void RegisterByMethodName_KeepsDynamicProvider()
        {
            Context.Commands.RegisterCommand("dynamic-hints-test.by-name", "test command", nameof(PickDynamic), typeof(DynamicValueHints));

            Assert.That(Hint("dynamic-hints-test.by-name", "value").DynamicValueProviderCommands, Is.EqualTo(new[] { Provider }));
        }

        [Test]
        public void RegisterByDelegate_KeepsDynamicProvider()
        {
            Context.Commands.RegisterCommand<string>("dynamic-hints-test.by-delegate", "test command", PickDynamic);

            Assert.That(Hint("dynamic-hints-test.by-delegate", "value").DynamicValueProviderCommands, Is.EqualTo(new[] { Provider }));
        }

        [Test]
        public void MetaData_GetsOnlyStaticValues()
        {
            string[] seen = null;
            Context.Commands.RegisterCommand(
                "dynamic-hints-test.meta",
                "test command",
                nameof(PickDynamic),
                typeof(DynamicValueHints),
                meta => seen = meta.Options[0].AvailableValues.ToArray()
            );

            Assert.That(seen, Is.Empty);
        }

        [Test]
        public void MetaData_AddedValueJoinsStaticValues()
        {
            Context.Commands.RegisterCommand(
                "dynamic-hints-test.static",
                "test command",
                nameof(PickBlade),
                typeof(DynamicValueHints),
                meta => meta.Options[0].AvailableValues.Add("Spear")
            );

            Assert.That(
                Hint("dynamic-hints-test.static", "blade"),
                Is.EquivalentTo(new[] { nameof(Blade.FalcataIron), nameof(Blade.GladiusCopper), "Spear" })
            );
        }

        private AvailableValuesHint Hint(string commandName, string optionName)
        {
            var command = Context.Commands.Commands[commandName];
            return command.ValueHints[command.Options[optionName]];
        }
    }
}
