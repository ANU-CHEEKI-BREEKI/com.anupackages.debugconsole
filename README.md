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

## Basic command line syntax
NDeskOptions + C# optional and named parameters style

commands categories
commands list
UI and UI commands



## Expression evaluation

`ATTENTION:` to use ExpressionEvaluation install package:

original repo: [NCalc](https://github.com/ncalc/ncalc)

## #defines



## Nested commands




## Attributes

InstanceTargetType

### Static 
### Instanced

## Direct registration

# Advanced

## Custom converters

## Custom commandline preprocessors

## UI Themes

# License
[MIT licensed](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/blob/log-list/LICENCE.md).
