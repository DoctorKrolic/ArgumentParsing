# Changelog

## 0.3.0

This release brings customizability to special commands and other miscellaneous improvements.

- It is now possible to add additional special commands to the parser
- It is now possible to disable/replace built-in special commands
- Default help screen can now be changed to a custom one
- Parse error messages can now be localized/changed
- `DateTime`, `DateOnly`, `TimeSpan` and `TimeOnly` are now valid types for options type members

## 0.2.0

This release focuses on addressing technical dept and improving developer experience. It doesn't include any additional library features besides an important breaking change.

### Added:
- 9 new diagnostics, including 2 errors for cases that were previously possible but led to incorrect/confusing behavior, 2 warnings, 3 infos and 2 unnecessary markers
- Many diagnostics now come with corresponding code fixes, so if you see a diagnostic from the library in your code, first try to invoke the code fixes menu to see whether the library already has a good solution for you
- Nullability warnings from the C# compiler are now suppressed for sequence options and remaining parameters inside options types
- Code generation has been separated from code analysis, so you may expect better responsiveness in your IDE

### Changed:
- All options types must now be annotated with the `[OptionsType]` attribute from the `ArgumentParsing` namespace. If a type used in argument parsing is not annotated with this attribute, an error is reported and no code is generated. This change enables the separation of code analysis from code generation, allowing for the efficient implementation of many features. This release already includes several features that heavily rely on the `[OptionsType]` attribute and would probably be impossible without it. Given the numerous benefits this change brings and the fact that the library is still in early development, the breaking change is considered justified

## 0.1.0

Initial release of the library.

### Features:
- Generation of argument parser methods
- Generated parsers can parse options, parameters and special commands
- Options are named value, which syntax follows GNU syntax conventions
- Parameters are unbound values, which are captured from arguments based on parameters' indices
- Special commands are parsed when a particular first argument is encountered
- Default implementation of `--help` and `--version` special commands
