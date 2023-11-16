# com.anupackages.debugconsole
*Advanced in-game console for Unity 3D, which replicates Windows cmd.*

![small-cover](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/f550ef6b-0975-4231-8228-fe37b7a222dc)

![cover-gif](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/c0134944-47a8-45cd-b5e4-caa5071dbf67)


- [ATTENTION](#ATTENTION)
- [About](#About)
- [How to Install](#How-to-Install)
- [Roadmap](#Roadmap)
- [How to use](#How-to-use)
  - [Attributes](#Attributes)
  - [Direct registration](#Direct-registration)
- [License](#License)


# ATTENTION

This Asset is depends of other [repo](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.ndesk-options), which is redistribution of MIT licensed C# options parser [NDesk.Options](http://gitweb.ndesk.org/?p=ndesk-options;a=summary) (looks like original repo already missed, but there is [original web page](http://www.ndesk.org/Options))

If you install this asset as package by Unity Package Manager, than packcage com.anupackages.ndesk-options will be installed automatically.

Optional dependency:
- if you want to use Expression Evaluation, you need to install following packages. Whis are also redistribution for Unity Package Manager usage. See more in [How to Install](#How-to-Install)
  - [com.anupackages.debugconsole.expressions](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole.expressions) - enables  Expression Evaluation
  - [NCalc](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.ncalc)
  - [ANTLR v4](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.antlr4)

# About
Provides easy way to create and invoke debug commands in game console at runtime.
Also it displays console messages (logs, warnings, errors, exceptions, assertions) at runtime in a build.

User interface is created with uGUI and packed in a single SpriteAtlas. 

- Commands, parameters, suggestions and auto complete\
  <img width="410" alt="" src="https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/c2713c6b-a7b9-404a-a766-162f9b868192">
  <img width="204" alt="" src="https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/caeed8f5-dc7c-47d6-892d-19547ea7c7b2">
  <img width="189" alt="" src="https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/f195cd02-a642-435d-bbcc-fa09843f934d">

- #defines
- Expression evaluation
- Nested commands

- Persistent commands history\
  <img width="410" alt="" src="https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/2d4c2238-5036-4187-9854-2f0b4ee9001c">

- It is possible to resize or change UI scale of the console window during the game.\
  <img width="150" alt="" src="https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/5f56fb2b-8a6d-4045-ab25-ae19ccd657ef">
  
- Console messages can be filtered by message type the same way as in UnityEditor console window.\
  <img width="200" alt="" src="https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/1bbee44d-cbc2-41dd-b06e-e576b52394fb">
  
- It is possible to filter logs by search query ste same way as in UnityEditor console window.\

- Infinite scroll. Logs scroll list are recyclable. So you can have ponentially infinite amoung of logs in the console

# How to Install

- through Unity Package Manager as ***npm package*** (*preffered method*): add following to your Scoped Registries
  ```
    "scopedRegistries": [
      {
        "name": "ANU",
        "url": "https://registry.npmjs.org/",
        "scopes": [
          "com.anupackages"
        ]
      }
    ]
  ```
  <img width="809" alt="Screenshot 2023-06-28 at 19 28 06" src="https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/a967a238-a5d5-41d8-8bcc-23e8c575dfcc">

  Then you will be able to search all available packages directly in Unity Package Manager window
  <img width="717" alt="Screenshot 2023-06-28 at 19 28 52" src="https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/d8aa5f5c-8911-4d03-a648-2351083dc764">

  When installing package with dependencies theought regustries, all dependencies will be installed automatically.

  `Do note, that NCalc for Expression evaluations it an optional dependency. So if you want to use Expression evaluations, you need to install it manually.`

- through Unity Package Manager from git url:
  
  Add all dependencies to your manifest.json:\
  mandatory:
  ```
    "com.unity.textmeshpro": "3.0.6",
    "com.anupackages.debugconsole": "https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole.git",
    "com.anupackages.ndesk-options": "https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.ndesk-options.git",
  ```
    optional:
  ```
    "com.anupackages.debugconsole.expressions": "https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole.expressions.git"
    "com.anupackages.ncalc": "https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.ncalc.git",
    "com.anupackages.antlr4": "https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.antlr4.git",
  ```
- download sources as zip archive and import to your project (you will need then to manually download all dependencies too)
- [from asset store](https://u3d.as/36S8) (updates less frequently because of long unity review time and general publishing process)
  
# Roadmap
Each star ★ on the project page brings new features closer. You can suggest new features in the [Discussions](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/discussions).

# How to use

## How To Begin

- Just drag and drop DebugConsole prefab to the scene. And you are ready to go.
- Use Tilde `~` key (or BackQuote `` ` ``) to open and close the console UI.
- Use `Ctrl` + `~` to switch suggestion context (*Command line History* or *Commands Suggestions*)
- Use `Ctrl` + `.` to force open suggestion popup even if command line is empty
- Use `Tab` to apply first suggestion from popup
- Use `Arrow Up` and `Arrow Down` to scroll suggestions
- Use `Tab` or `Enter` to apply selected suggestion
- Use `Esc` to move focus back from suggestion popup to command line

## Adding Commands

### Attributes
See more attributes in the [wiki]()

----

- Simply add `[DebugCommand]` attribute to field, property ot method, and it will become the command accessible from the 
console!
- Add `[DebugCommandPrefix("prefix")]` attribute to the class which contains the commands, and all commands will have `prefix.` before their names

***Example:***
```cs
using UnityEngine;
using ANU.IngameDebug.Console;

[DebugCommandPrefix("example")]
public class MyScript : MonoBehaviour 
{
    [DebugCommand]
    [SerializeField] private int _myField;    

    [DebugCommand]
    public int Property { get; set; }

    [DebugCommand]
    public void MethodWithParameters(int num, Vector4[] arr, bool flag = false){ }
}
```

From the code above will be registered following commands:
- `example.my-field [value]`
- `example.property [value]`
- `example.method-with-parameters num arr [flag]`

Field and property commands has default optional parameter `value`.\
If you call the command without parameter, command will  return field/property value.\
If you pass the value, command will set this vale to field/property.

By default, *DebugConsole* search end register commands only in types that inherit `MonoBehaviour`. If you want to declare your commands in any other type, you should register that type by declaring `[RegisterDebugCommandTypes]` attribute on the assembly. But in this case you should use only static commands or register instance of that type in `DebugConsole.InstanceTargets` registry. See more in [InstanceTargetType usage](#instanced-commands)

```cs
using UnityEngine;
using ANU.IngameDebug.Console;

[assembly: RegisterDebugCommandTypes(typeof(NonMonoBehaviorClass))]

public class NonMonoBehaviorClass
{
  [DebugCommand]
  public static void StaticMethod(){}
  
  [DebugCommand]
  public void InstanceMethod(){}

  [RuntimeInitializeOnLoadMethod]
  private static void InitStatic() 
    => DebugConsole.InstanceTargets.Register(new NonMonoBehaviorClass());
}
```



There are a lot more [attributes available]().\ You can
- Change command name [[DebugCommand(Name="name")]]()
- Add command description [[DebugCommand(Description="desc")]]()
- Add method parameter alternative names [[OptAltNames("h","?")]]()
- Add method parameter description [[OptDesc("parameter description")]]()
- Add parameters available values hint 
  - as constant collection [[OptVal(v1, v2, v3)]]() 
  - or dynamic collection [[OptValDynamic("list-values")]]()<br>
  for dynamic values provider nested command execution are used<br>
    <details>
      <summary>Click to expand</summary>
      
      ```cs
      [DebugCommand]
      private static void LoadScene(
          [OptAltNames("n")]
          [OptDesc("Load scene by name")]
          [OptValDynamic("default.list-scene-names")]
          string name = "",
          [OptAltNames("i")]
          [OptDesc("Load scene by index")]
          [OptValDynamic("default.list-scene-indices")]
          int index = -1,
          [OptAltNames("r")]
          [OptDesc("Set only this flag to reload current scene")]
          bool reload = false
      )
      {
          if (!string.IsNullOrEmpty(name))
              SceneManager.LoadScene(name);
          else if (index >= 0)
              SceneManager.LoadScene(index);
          else if (reload)
              SceneManager.LoadScene(SceneManager.GetActiveScene().name);
          else
              throw new System.Exception("Pass at least one parameter");
      }

      [DebugCommand]
      private static IEnumerable<string> ListSceneNames()
          => ListSceneIndices().Select(i => SceneUtility
              .GetScenePathByBuildIndex(i)
              .Split('/')
              .LastOrDefault()
              ?.Split('.')
              ?.FirstOrDefault()
          );

      [DebugCommand]
      private static IEnumerable<int> ListSceneIndices()
      {
          var cnt = SceneManager.sceneCountInBuildSettings;
          for (int i = 0; i < cnt; i++)
              yield return i;
      }
      ```
    </details>
  


### Direct registration

----

You can also register commands dynamically by `DebugConsole API`

Strongly typed
---

- Lambda commands:
```cs
DebugConsole.Commands.RegisterCommand<int, bool>("is-positive", "Is the number greater than 0", num => num > 0)
DebugConsole.Commands.RegisterCommand<float>("time-scale", "Set time scale", value => Time.scale = value)
```
- Delegate commands (can be decorated by attributes, the same way as in [Attributes](#Attributes) section)
```cs
using System;
using ANU.IngameDebug.Console;

class NonMbClass
{
  [DebugCommand(Name="custom-name-instanced", Description="Custom description")]
  public static void InstanceMethod(
    [OptDesc("integer value in range [0; 3]")]
    [OptAltNames("p")]
    [OptVal(0, 1, 2, 3)]
    int parameter
  ){}

  [DebugCommand(Name="custom-name-static", Description="Custom description")]
  public static void StaticMethod(){}
}
```
```cs
var instance = new NonMbClass();
DebugConsole.Commands.RegisterCommand(new Action<int>(instance.InstanceMethod))
DebugConsole.Commands.RegisterCommand(new Action(NonMbClass.StaticMethod))
```

Weakly typed
---

- Method info
```cs
class MyClass
{
  public void MyMethod(int arg1, Quaternion arg2){}
}
```
```cs
var methodInfo = typeof(MyClass).GetMethod("MyMethod");
DebugConsole.Commands.RegisterCommand("by-method-info", "desc", methodInfo, new MyClass());
```

- By static method name
```cs
class MyClass
{
  static void MyStaticMethod(int arg1, Quaternion arg2){}
}
```
```cs
DebugConsole.Commands.RegisterCommand("static-by-name", "desc", "MyStaticMethod", typeof(MyClass));
```

- By instance method name
```cs
class MyClass
{
  void MyMethod(int arg1, Quaternion arg2){}
}
```
```cs
var instance = new MyClass();
DebugConsole.Commands.RegisterCommand("instanced-by-name", "desc", "MyMethod", instance);
```

#### ***Static commands***
It is simplest case of the command. No instance ot type, where command declared, required to call the command.

#### ***Instanced commands***

When using `[DebugCommand]` attribute, all methods, properties and fields marked by this attribute and declared directly inside any MonoBehaviour automatically registered.\
You can also use `[RegisterDebugCommandTypes]` attribute to register the non MonoBehaviour types, to find and register all commands declared inside that types.

However, to call this commands, instance of type where commands declared is required.

You can pass this instance directly in the console as command target (`For all instanced commands option names [--targets|t] are reserved for this purpose`), or you can let the *DebugConsole* to find the instances for you.

There is an optional property `InstanceTargetType Target` in `[DebugCommand]` attribute, which let you to define how the target command instance should be found.

#### ***InstanceTargetType***
- **AllActive** - *DebugConsole* will use GameObject.Find to find all components on active game objects and call the command for each of them
- **AllIncludingInactive** - *DebugConsole* will use GameObject.Find to find all components on active and inactive game objects and call the command for each of them
- **FirstActive** - *DebugConsole* will use GameObject.Find to find first component on active game objects and call the command for it
- **FirstIncludingInactive** - *DebugConsole* will use GameObject.Find to find first component on active or and inactive game objects and call the command for it
- **Registry** - *DebugConsole* will search for instances of target type only in `DebugConsole.InstanceTargets` registry.\
When the command declared inside non MonoBehaviour type, `InstanceTargetType.Registry` always be used, no matter what value you set in `DebugCommandAttribute.Target` property

## Basic command line syntax

Basic syntax based is based on [NDeskOptions](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.ndesk-options) package, and was extended to to support unnamed parameters. From that point it becomes similar to `C# named parameters` syntax. 

### Parameters
----
*Lest see the example command:*

```cs
using UnityEngine;
using ANU.IngameDebug.Console;

public class MyScript : MonoBehaviour 
{
    [DebugCommand]
    public void Command(
      int param1, 
      [OptAltNames("p")]
      float param2 = 1.5f, 
      bool optionalBool = true,
      [OptAltNames("f")]
      bool firstFlag = false,
      [OptAltNames("s")]
      bool secondFlag = false,
      // do note: t is reserved name for instanced commands. so use 2nd symbol as alt name
      [OptAltNames("h")]
      bool thirdFlag = false){ }
}
```

That method are accessible from the console in syntax:
```
command param1 [param2|p=1.5] [optionalBool=true] [firstFlag|f] [secondFlag|s] [thirdFlag|h]
```
*You should pass all required parameters.<br>*
*And you can skip optional parameters.<br>*
***But you should use of the following rules:<br>***

- pass all parameters without specifying parameter names in order they declared in the corresponding method<br>
```
command 123 12 false
```
is equivalent to C# code:
```cs
myScript.Command(123, 12, false);
```
- pass all parameters with parameter names, what allows you to pass them in any order.
```
command --param2=12, --param1=123 --optionalBool=false
```
is equivalent to C# code:
```cs
myScript.Command(param2: 12, param1: 123, optionalBool: false);
```
- combine two approaches like in C# named parameters:
  1. pass any parameters count without names in same order that parameters declared in the method
  1. pass other parameters in any order you want, specified their names
  1. if any parameter passed with name - all following parameters must be passed with names too

```
command 123, --optionalBool=false, --param2=12
```
is equivalent to C# code:
```cs
myScript.Command(123, optionalBool: false, param2: 12);
```

### Flags
---
Optional boolean parameter with default value `false` are considered as a `flag`.
It should be passed without specifying the value, since any different value from 'false' than you can pass is 'true'. <br>
So you should just pass
- flag name for named parameter
- ANY not empty string for unnamed parameter

so following commands are equivalent
```
command 123 --firstFlag --secondFlag --thirdFlag --param2=12, --optionalBool=false
command 123 12 false a a a
```
where `a a a` can be anything. like `a b c` or `yes aha sure`<br>
and equivalent to C# code
```cs
myScript.Command(123, firstFlag: true, secondFlag: true, thirdFlag: true, param2: 12, optionalBool: false);
myScript.Command(123, 12, true, true, true, true);
```

### Bundled flags
----
If flag has single character alternative name, it can be used in [bundled](http://www.ndesk.org/Options) mode.<br>
Bundled parameters must start with a single '-' and consists of a sequence of (optional) boolean flags followed by an (optional) parameter name nad followed by that parameter value. <br>

In this manner these commands are equivalent:
```cs
command 123 -fh
command 123 --firstFlag --thirdFlag
command 123 -f -h
```
and 
```cs
command 123 -fhp12
command 123 -f -h -p=12
```

## Supported parameter types:

- `string`
  - surround with `"` or `'`
- `bool` (non case sensitive)
  - true: `t, 1, yes, y, approve, apply, on`
  - false: `f, 0, no, n, discard, cancel, off`
- `Quaternion`
  - parsed as `Vector3` euler angles
- `Color32`
  - parsed as `Color`
- `Color`
  - parsed as `Vector3`
  - parsed as `Vector4`
  - parsed as [HtmlString](https://docs.unity3d.com/ScriptReference/ColorUtility.TryParseHtmlString.html), for example `#RGB`, `#RRGGBBAA`, `red`, `cyan`, etc..
- array or list
  - `[]` or `()` are equivalent, so below listed samples for `[]` only
  - `,` or ` ` as component delimiter: `[1, 2, 3]` or `[1 2 3]` or `[1  , 2   ,3   ]` are equivalent
  - `[n]` or just `n` without `[]`: single item `n`
  - `[]`: empty collection
- vectors (`Vector2Int`, `Vector2`, `Vector3Int`, `Vector3`, `Vector4`)
  - parsed as array of fixed size
  - `[]`: all components equals zero
  - `[n]` or just `n` without `[]`: all components equals `n`
- GameObject and Component, any type inherited from Component
  - used `GameObject.Find` and filtered by `name` for Component types
  - can pass `null` (non case sensitive)

## Expression evaluation

***`ATTENTION:`*** to use ExpressionEvaluation install [com.anupackages.debugconsole.expressions](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole.expressions).

When ExpressionEvaluation installed, you can evaluate expressions directly in console command line. The syntax is similar to [Unity ExpressionEvaluator.Evaluate](https://docs.unity3d.com/ScriptReference/ExpressionEvaluator.Evaluate.html) which is used to evaluate expressions directly in inspector numeric fields. So if you have used this at least one time, you should be already familiar with this.

***For example:*** following command line inputs are equivalent:
```cs
command --param1="(1 + 3)*(1 + 1)"
command --param1=(1+3)*(1+1)
command (1+3)*(1+1)
command 8
```

- Subset of System.Math methods are supported (Log, Max, Min, Round, Abs, Sin, Cos [and many more](https://github.com/ncalc/ncalc/wiki/Functions))
- UnityEngine.Mathf.Clamp and Clamp01 are supported
- Mathf.PI, Mathf.Deg2Rad, Mathf.Rad2Deg constants are supported

## #defines

Defines, like C macros, allow you to create shorthands which will then be expanded before the command is parsed. Defines are defined using `#define` command in console command line or by `DebugConsole.Defines.Add` API

```cs
#define num 125
```
equivalent to C# code:
```cs
DebugConsole.Defines.Add("num", "125");
```
and 
```cs
#define num 125
#define a 2
#define b 4
#define apb #a+#b
command #num
command #apb
```
equivalent to
```cs
command 125
command 6
```

## Nested commands
You can surround expression in brackets `{}` to parse it like nested command. The expression will be invoked recursively, with its result bubbled up to the expression.

```cs
[DebugCommandPrefix("player")]
public class Player
{
  [DebugCommand]
  [SerializedField] private float _hp = 10;
  
  [DebugCommand]
  [SerializedField] public float MaxHp => 10;
  
  [DebugCommand]
  public void Heal(float amount) => _hp += amount;

  [DebugCommand]
  public void InstanceMethod(){}
}
```

Set player hp to max value:
```
player.hp {player.max-hp}
```

Define shortcut to calculate 25% of player max hp<br>
And heal player by 25% of max value
```
#define hp25percent {player.max-hp}*0.25
player.heal #hp25percent
```

# Advanced

## Custom converters
----
You can add custom converters to support more types parsing.

- For types hierarchy
  - you should implement `IIConverter` interface

```cs
public class ComponentConverter : IConverter
{
    public Type TargetType => typeof(UnityEngine.Component);

    object IConverter.ConvertFromString(string option, System.Type targetType)
    {
        if (option.ToLower() == "null")
            return null;
        
        return GameObject
          .FindObjectsOfType(targetType)
          .FirstOrDefault(t => t.name == option);
    }
}
```
```cs
Converters.Register(new ComponentConverter());
```
- for concrete type 
  - you should implement `IConverter<T>`
```cs
public class BoolConverter : IConverter<bool>
{
    public bool ConvertFromString(string option)
    {
        switch (option.ToLower())
        {
            case "0":
            case "false":
            case "f":
                return false;
            case "1":
            case "true":
            case "t":
                return true;
            default:
                throw new Exception($"Not a valid input for Boolean: {option}");
        }
    }
}
```
```cs
Converters.Register(new BoolConverter());
```
  - or you can use lambda converter
```cs
Converters.Register<bool>(option => option == "1" || option == "true" || option == "t");
```

## Custom command line preprocessor
----
You can write and register own `ICommandInputPreprocessor` implementation to extend command line syntax.
After entered command recorded to the command history, but before the command parsed and executed it being proceeded by `DebugConsole.Preprocessors`.

All implemented syntax extensions made by `ICommandInputPreprocessor` implementations:
- arrays brackets support - `BracketsToStringPreprocessor`
- unnamed parameters - `NamedParametersPreprocessor`
- defies - `DefinesPreprocessor`
- expression evaluations - `ExpressionEvaluatorPreprocessor`
- nested commands - `NestedCommandsPreprocessor`

Basically it just grabs the input. ang changed it that way to support already existing syntax.<br>
For example `BracketsToStringPreprocessor` just surround `[]` or `()` brackets by `"`, and make it string parameter, so NDescOption can parse it and pass to corresponding parameter as single value, not splitting by whitespace.

```cs
public class BracketsToStringPreprocessor : ICommandInputPreprocessor
{
    private readonly Regex _regex = new Regex(@"(?<!""|')\s+(?<content>(\[.*?\])|(\(.*?\)))");

    public string Preprocess(string input)
    {
        var matches = _regex.Matches(input);
        input = _regex.Replace(input, @" ""${content}""");
        return input;
    }
}
```
```cs
DebugConsole.Processors.Add(new BracketsToStringPreprocessor());
```

## IInjectDebugConsoleContext
----
Sometimes you want to access already registered `Converters`, `Processors`, or even silently execute command from inside `IConverter`or `ICommandInputPreprocessor`. 

Technically you can access it by `DebugConsole.Converters` or `DebugConsole.Processors`, but its not recommended due to static spaghetti code and inability to write unit tests.

For that purpose implement `IInjectDebugConsoleContext` interface, and `IReadOnlyDebugConsoleProcessor` will be injected to  your converter or processor.

***For example:*** there are implementation of `Color32Converter`. It just parse the input as Color and then uses it to construct Color32

```cs
public class Color32Converter : IConverter<Color32>, IInjectDebugConsoleContext
{
    IReadOnlyDebugConsoleProcessor IInjectDebugConsoleContext.Context { get; set; }
    private IReadOnlyConverterRegistry Converters => Context.Converters;

    public Color32 ConvertFromString(string option)
    {
        var color = Converters.ConvertFromString<Color>(option);
        return new Color32(
          (byte)(color.r * 255), 
          (byte)(color.g * 255), 
          (byte)(color.b * 255), 
          (byte)(color.a * 255)
        );
    }
}
```

## UI Themes


# Third party notices

# License
[MIT licensed](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/blob/log-list/LICENCE.md).
