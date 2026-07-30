using System;

namespace VersePulse.App.Telemetry.Parsing.Parsers;

public sealed class GameStateParser : ILogParser
{
    public void Parse(string line, TelemetryData telemetry)
    {
        if (line.Contains("Join PU", StringComparison.OrdinalIgnoreCase))
        {
            telemetry.GameState = GameState.Loading;
            telemetry.LastEvent = "Joining Persistent Universe";
            return;
        }

        if (line.Contains(
                "Session Manager [Request Connect]",
                StringComparison.OrdinalIgnoreCase)
            || line.Contains(
                "Connect started",
                StringComparison.OrdinalIgnoreCase)
            || line.Contains(
                "Expect Incoming Connection",
                StringComparison.OrdinalIgnoreCase))
        {
            telemetry.GameState = GameState.Loading;
            telemetry.LastEvent = "Connecting to game server";
            return;
        }

        if (line.Contains(
                "taskname=\"InGame\"",
                StringComparison.OrdinalIgnoreCase)
            || line.Contains(
                "EGameContextState::eEGS_Running",
                StringComparison.OrdinalIgnoreCase))
        {
            telemetry.GameState = GameState.InServer;
            telemetry.LastEvent = "Entered game server";
            return;
        }

        if (line.Contains(
                "RequestFrontEnd",
                StringComparison.OrdinalIgnoreCase)
            || line.Contains(
                "Loading GameModeRecord='SC_Frontend'",
                StringComparison.OrdinalIgnoreCase))
        {
            telemetry.GameState = GameState.MainMenu;
            telemetry.LastEvent = "Entered main menu";
        }
    }
}
