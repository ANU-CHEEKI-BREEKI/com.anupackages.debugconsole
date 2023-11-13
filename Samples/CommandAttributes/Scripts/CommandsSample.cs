using System.Collections;
using System.Collections.Generic;
using ANU.IngameDebug.Console;
using UnityEngine;

[DebugCommandPrefix("test-commands")]
public class CommandsSample : MonoBehaviour
{
    [DebugCommand][field: SerializeField] public bool Prop2 { get; set; } = true;
    [DebugCommand][SerializeField] private TwoEnum _twoEnum;
    [DebugCommand][SerializeField] private ThreeEnum _threeEnum;
    [DebugCommand][SerializeField] private FourEnum _fourEnum;
    [DebugCommand][SerializeField] private string _stringValue;
    [DebugCommand][SerializeField][OptValDynamic("test-commands.list-string-values")] private string _stringValueFromList;

    private enum TwoEnum { FirstValue, SecondValue }
    private enum ThreeEnum { OneOfThree, SecondName, ThirdValue }
    private enum FourEnum { OneOfFour, TwoOfFour, Third, Fourth }

    [DebugCommand] private void SetTwoEnum(TwoEnum value) => _twoEnum = value;
    [DebugCommand] private void SetThreeEnum(ThreeEnum value) => _threeEnum = value;
    [DebugCommand] private void SetFourEnum(FourEnum value) => _fourEnum = value;

    [DebugCommand] private TwoEnum GetTwoEnum() => _twoEnum;
    [DebugCommand] private ThreeEnum GetThreeEnum() => _threeEnum;
    [DebugCommand] private FourEnum GetFourEnum() => _fourEnum;

    [DebugCommand]
    private void SetStringFromList(
        [OptValDynamic("test-commands.list-string-values")]
        string value
    ) => _stringValue = value;

    [DebugCommand]
    private void SetStringFromListDefault(
        [OptValDynamic("test-commands.list-string-values")]
        string value = "default value not from list"
    ) => _stringValue = value;

    [DebugCommand]
    private IEnumerable<string> ListStringValues()
    {
        yield return "first string value";
        yield return "second";
        yield return "third value longer than first one";
        yield return "fourth visible";
        yield return "last one?";
        yield return "no";
        yield return "we gonna have";
        yield return "long list of values";
    }

    [DebugCommand(Platforms = TargetPlatforms.PC)] private void Cmd_Pc() { }
    [DebugCommand(Platforms = TargetPlatforms.Mobile)] private void Cmd_Mobile() { }
    [DebugCommand(Platforms = TargetPlatforms.Editor)] private void Cmd_Editor() { }
    [DebugCommand(Platforms = TargetPlatforms.PC | TargetPlatforms.Mobile)] private void Cmd_Pv_Mobile() { }

    [DebugCommand(Platforms = TargetPlatforms.Any)]
    private bool AnyPlatform() => true;

    [DebugCommand(DisplayOptions = CommandDisplayOptions.None)]
    private bool ExcludedFromSuggestions() => true;

    [DebugCommand]
    private void RegisterDummyCommands()
    {
        for (int i = 0; i < 10; i++)
            DebugConsole.Commands.RegisterCommand($"dummy.dummy-command-{i + 1}", "", () => { });
    }
}
