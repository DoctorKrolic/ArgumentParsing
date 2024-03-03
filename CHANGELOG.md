# Changelog

## 0.1.0

Initial release of the library.

### Features:
- Generation of argument parser methods
- Generated parsers can parse options, parameters and special commands
- Options are named value, which syntax follows GNU syntax conventions
- Parameters are unbound values, which are captured from arguments based on parameters' indices
- Special commands are parsed when a particular first argument is encountered
- Default implementation of `--help` and `--version` special commands
