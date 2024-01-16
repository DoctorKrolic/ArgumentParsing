﻿namespace ArgumentParsing.Generators.Models;

internal sealed record OptionInfo(string PropertyName, string Type, char? ShortName, string LongName, ParseStrategy ParseStrategy);
