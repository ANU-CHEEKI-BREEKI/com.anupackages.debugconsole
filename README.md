## com.anupackages.debugconsole
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


## ATTENTION

This Asset is depends of other [repo](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.ndesk-options), which is redistribution of MIT licensed C# options parser [NDesk.Options](http://gitweb.ndesk.org/?p=ndesk-options;a=summary) (looks like original repo already missed, but there is [original web page](http://www.ndesk.org/Options))

If you install this asset as package by Unity Package Manager, than packcage com.anupackages.ndesk-options will be installed automatically.

## About
Provides easy way to create and invoke debug commands in game console at runtime.
Also it displays console messages (logs, warnings, errors, exceptions, assertions) at runtime in a build.

User interface is created with uGUI and packed in a single SpriteAtlas. 
- Commands, parameters, parameters values autocomplete\
  <img width="410" alt="" src="https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/c2713c6b-a7b9-404a-a766-162f9b868192">
  <img width="204" alt="" src="https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/caeed8f5-dc7c-47d6-892d-19547ea7c7b2">
  <img width="189" alt="" src="https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/f195cd02-a642-435d-bbcc-fa09843f934d">
  
- Persistent commands history\
  <img width="410" alt="" src="https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/2d4c2238-5036-4187-9854-2f0b4ee9001c">

- It is possible to resize or change UI scale of the console window during the game.\
  <img width="150" alt="" src="https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/5f56fb2b-8a6d-4045-ab25-ae19ccd657ef">
  
- Console messages can be filtered by message type the same way as in UnityEditor console window.\
  <img width="200" alt="" src="https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/assets/15821105/1bbee44d-cbc2-41dd-b06e-e576b52394fb">
  
- It is possible to filter logs by search query ste same way as in UnityEditor console window.\

## How to Install

- via Unity Package Manager
  - git url: `https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole.git`
  - npm package: ``
- download sources as zip archive and import to your project
- asset store?
  
## Roadmap
Each star ★ on the project page brings new features closer. You can suggest new features in the [Discussions](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/discussions).

## How to use

Supported parameter types and some syntax flexibility:
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

### command line syntax
NDeskOptions + C# optional and named parameters style


commands categories
commands list
UI and UI commands
Infinite scroll

### Attributes

InstanceTargetType

#### Static 
#### Instanced

### Direct registration

### Custom converters


### public ILogger Logger { get; set; }

### Custom commandline preprocessors

### UI Themes

## License
[MIT licensed](https://github.com/ANU-CHEEKI-BREEKI/com.anupackages.debugconsole/blob/log-list/LICENCE.md).
