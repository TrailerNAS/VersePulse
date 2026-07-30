using System;
using System.Collections.Generic;

namespace VersePulse.App.Telemetry.Parsing;

public sealed class ParserPipeline
{
    private readonly IReadOnlyList<ILogParser> _parsers;

    public ParserPipeline(IEnumerable<ILogParser> parsers)
    {
        ArgumentNullException.ThrowIfNull(parsers);
        _parsers = [.. parsers];
    }

    public void Parse(string line, TelemetryData telemetry)
    {
        ArgumentNullException.ThrowIfNull(line);
        ArgumentNullException.ThrowIfNull(telemetry);

        foreach (ILogParser parser in _parsers)
        {
            parser.Parse(line, telemetry);
        }
    }
}
