# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Changed

## [2.3.0] - 2026-11-22

### Added
- the `[DebugCommand]` member list is made in the editor at build time: `CommandListBuildStep` scans the assemblies the player gets (no editor and no test assemblies) that reference the console, writes the members into a Resources text file for the build only, fails the build if the list reads back to other commands, and keeps the listed members from the linker with a generated link.xml. The player resolves only the listed members by name, without scanning any assembly (on an IL2CPP Android phone the scan took about 4 minutes, the list takes milliseconds)
- Project Settings > ANU Debug Console: which builds get the list (Development Builds by default, builds with given define symbols, all builds, never)
- `CommandMembers`: the scan, the list format and the command creation as a public API

### Changed
- without the list (the editor, or a build without it) commands are searched only in the console assembly and the assemblies that reference it, skipping members inherited from other assemblies, instead of every member of every loaded assembly; the commands found are the same
- `AttributeCommandsInitializerProcessor.Initialize` logs one line with the command count, the source and the time instead of the per-kind timings; `Initialize(string buildList)` registers from a list
- console commands nothing calls (`#define`, `echo`, `time-scale`, ...) are no longer stripped from IL2CPP builds that have the list

## [2.2.2] - 2026-11-17

### Fixed
- `[OptValDynamic]` value suggestions work for commands registered with `ICommandsRegistry.RegisterCommand` by delegate, `MethodInfo` or method name: the dynamic value provider commands are kept and asked each time, not run once at registration
- the `metaDataCustomize` callback of those overloads gets only the static values in `AvailableValues`

## [2.2.1] - 2026-09-14

### Changed
- Unity 6.4+ support: `ObjectInfo` prints `GetEntityId()` instead of the obsolete `GetInstanceID()`; older Unity versions keep `GetInstanceID()`
- Unity 6.6 support: `DashboardLayout` passes the maximum size required by the new `LayoutGroup.SetLayoutInputForAxis` (com.unity.ugui 2.6.0); older Unity versions keep the old call
- sprite metas reserialized by Unity 6.6

## [2.2.0] - 2026-08-31

### Added
- WebGL target platform flag
- strict device-platform check: the floating button hidden in the editor unless the Editor flag is set (prefab default is now Mobile)
- suggestion search ignores case and matches typed terms in any order
- editor tests for suggestions, platform checks; command line parser fixes covered by the SupportedTypes suite

### Changed
- applying a value suggestion replaces the partially typed value instead of appending after it; values with spaces are quoted
- command line supports single-quoted arguments; a bare "-" and negative numbers parse as values, not option names
- nested collection values like ((0.2, 5) (35.4, 29)) parse correctly
- dropped the com.unity.textmeshpro dependency for Unity 6 compatibility; on Unity 2022 and older install TextMeshPro manually
- fixed infinite recursion in InstancesTargetRegistry.Unregister
- OnDestroy unsubscribes the log handler that was actually subscribed and clears the singleton
- Task.Run is skipped at runtime on WebGL (no thread pool there)
- BREAKING: DeleteAllChild extension renamed to DestroyAllChild; Color.ToHexString extension removed
- bug fix of Editor Enter Play Mode Settings support
- compilation fix for non Android target platforms

## [2.1.3] - 2023-11-16
## [2.1.2]
### Added

### Changed
- Expression Evaluation bugfixes
    - removed dependencies on com.anupackages.ncalc
    - moved Expression Evaluation preprocessor to com.anupackages.debugconsole.expressions
    - removed USE_NCALC directives

## [2.1.1] - 2023-11-15
### Added
- console message counters and last log message in bot of the dashboard

### Changed
- silenced exceptions during dashboard initialization for non static commands
- fixed "silence" commands execution
- console.set-theme command. removed unnecessary args. auto complete now able to handle argument values selection quickly
- set scale command for dashboard specific
- load scene. removed unnecessary args. separated to 2 commands for load and reload scene
- dropdown out of screen bug fix

## [2.1.0] - 2023-11-13
### Added
- mobile ui support - Dashboard commands view
    - grouping by command prefix
    - filtering by command prefix
    - filtering by command type (method, property, field)
    - easy accessible zero- and one-argument command presenters
    - generic command presenters
- floating button for open console on android
- saving floating button position
- saving last opened console tab
- auto refresh console layout on phone orientation change
- commands registration on startup
    - Synchronous
    - Asynchronous
- TargetPlatforms for commands registration
- CommandDisplayOptions for displaying commands on dashboard or/and in command line suggestion
- samples for setup scene and register commands (with dashboard specific parameters)

### Changed
- removed console.switch-context command. its unnecessary
- bug fix of unimplemented OptValDynamicAttribute for Fields and Properties
- auto refreshing console size when open
- NullReferenceException bug fix when called command ObjectInfo for object with no parent
- bug fix of nameless parameters suggestions
- saving/loading last selected theme

## [2.0.4] - 2023-10-02
### Added
- Support for the New Unity Input System

## [2.0.3] - 2023-07-04
### Added
- methods to open and close console from code.<br>
can be useful, foe example, when you write the command to make screenshot. then probably you want to close the console first
- commands to open and close console

### Changed
- bug fix of NestedCommandsPreprocessor and DefinesPreprocessor caused by evaluating in working thread, which is not allowed when accessing to UnityEngine library

## [2.0.2] - 2023-07-03
### Changed
- bug fix of package.json unity version

## [2.0.1] - 2023-07-03
### Changed
- bug fix of package.json unity version

## [2.0.0] - 2023-07-03
### Added
- CHANGELOG.md
- documentation in `README.md`
- unit tests
- `ExecuteCommand` return value
- `ExecuteCommand` silent mode
- new command line syntax
    - support nameless parameters
- command line parameters suggestions
- #defines
- nested commands
- expression evaluation
- custom type converters
- custom command line input preprocessors
- more default commands
    - load scene
    - time scale
    - GameObject info
    - display hierarchy
    - etc...

### Changed
- prefabs structure
- new ui style
    - resize
    - rescale
    - filtering logs by type 
    - filtering logs by search string
    - logs stacktrace
    - logs time
    - infinite scroll list
- help, list, and other commands output
- DebugConsole.RegisterCommand syntax
- DebugConsole static functions moved to separate 'registries' 
    - DebugConsole.Commands
    - DebugConsole.Converters
    - DebugConsole.Processors
    - DebugConsole.Defines
    - DebugConsole.Defines
    - DebugConsole.Logger
    - etc...
- suggestions popup fuzzy search<br>
    *now you can type substrings from origin name*<br>
    *For example:* `consesi` will find command `con`sole.`se`t-`si`ze<br>

## [1.1.0] - 2023-06-12 
## [1.0.8] - 2022-04-15
## [preview versions] 
    dont use them

