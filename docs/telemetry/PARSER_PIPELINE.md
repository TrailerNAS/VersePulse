# Parser Pipeline

The parser pipeline receives new lines from `ILogReader` and forwards each line to every registered `ILogParser`.

## Current parsers

- `SessionParser`
- `EnvironmentParser`
- `GameStateParser`

## Rules

- Parsers never open `Game.log`.
- Parsers only process the line they receive.
- Parsers update the shared telemetry state.
- New telemetry domains should be implemented as separate parser classes.

## Data flow

```text
Game.log
   |
   v
LogReader
   |
   v
ParserPipeline
   |
   +--> SessionParser
   +--> EnvironmentParser
   +--> GameStateParser
   |
   v
TelemetryData
```
