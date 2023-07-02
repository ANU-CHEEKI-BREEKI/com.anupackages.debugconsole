# com.anupackages.debugconsole
*Advanced in-game console for Unity 3D, which replicates Windows cmd.*

<img width="432" alt="" src="https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/373256d9-ea6e-479f-a8ae-e29c0c3c55e8">

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

- through Unity Package Manager
  - git url: `https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole.git`
    You will need to install all dependencies first, ohervice Unity Packake Manager wont allow to install package. So the next method is preffered
  - ***npm package*** (*preffered method*): add following to your Scoped Registries
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

- download sources as zip archive and import to your project
- asset store?
  
# Roadmap
Each star ★ on the project page brings new features closer. You can suggest new features in the [Discussions](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/discussions).

# How to use

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
- Change command [name]()
- Add command [description]()
- Add method parameter [names alias]()
- Add method parameter [description]()
- Add parameters available values hint as [constant]() or [dynamic]() collections

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
NDeskOptions + C# optional and named parameters style

commands categories
commands list
UI and UI commands

## Supported parameter types and some syntax flexibility:
- `string`
  - surround with `"` or `'`
- `bool` (non case sensitive)
  - true: `1`, `+`, `yes`, `y`, `approve`, `apply`, `on`
  - false: `0`, `-`, `no`, `n`, `discard`, `cancel`, `off`
- `Quaternion`
  - parsed as `Vector3` euler angles
- `Color32`
  - parsed as `Color`
- `Color`
  - parsed as `Vector3`
  - parsed as `Vector4`
  - parsed as [HtmlString](https://docs.unity3d.com/ScriptReference/ColorUtility.TryParseHtmlString.html), for example `#RGB`, `#RRGGBBAA`, `red`, `cyan`, etc..
- array or list
  - `[]` or `()` are equivalented, so below listed samples for `[]` only
  - `,` or ` ` as component delimiter: `[1, 2, 3]` or `[1 2 3]` or `[1  , 2   ,3   ]` are equivalented
  - `[n]` or just `n` witout `[]`: single item `n`
  - `[]`: empty colection
- vectors (`Vector2Int`, `Vector2`, `Vector3Int`, `Vector3`, `Vector4`)
  - parced as array of fixed size
  - `[]`: all components equals zero
  - `[n]` or just `n` witout `[]`: all components equals `n`
- GameObject and Component, any type inherited from Component
  - used `GameObject.Find` and filtered by `name` for Component types
  - can pass `null` (non case sensitive)

## Expression evaluation

***ATTENTION:*** to use ExpressionEvaluation install package:

original repo: [NCalc](https://github.com/ncalc/ncalc)

## #defines

## Nested commands

# Advanced

## Custom converters

## Custom commandline preprocessors

## UI Themes

# Third party notices

# License
[MIT licensed](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/blob/log-list/LICENCE.md).
