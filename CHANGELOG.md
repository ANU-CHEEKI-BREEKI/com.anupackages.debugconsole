# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Changed
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

