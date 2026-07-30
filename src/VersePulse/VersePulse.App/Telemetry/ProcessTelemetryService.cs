using System;
using System.Diagnostics;
using System.Linq;

namespace VersePulse.App.Telemetry;

public sealed class ProcessTelemetryService : IDisposable
{
    private TimeSpan _previousProcessorTime;
    private DateTime _previousSampleTime;
    private int? _trackedProcessId;

    public void Update(TelemetryData telemetry)
    {
        ArgumentNullException.ThrowIfNull(telemetry);

        Process? process = GetStarCitizenProcess();

        if (process == null)
        {
            Reset();

            telemetry.CpuUsagePercent = null;
            telemetry.RamUsageMb = null;
            telemetry.CommittedMemoryMb = null;

            return;
        }

        try
        {
            TrackProcess(process);

            UpdateCpuUsage(process, telemetry);

            telemetry.RamUsageMb =
                process.WorkingSet64
                / 1024d
                / 1024d;

            telemetry.CommittedMemoryMb =
                process.PrivateMemorySize64
                / 1024d
                / 1024d;
        }
        catch
        {
            Reset();

            telemetry.CpuUsagePercent = null;
            telemetry.RamUsageMb = null;
            telemetry.CommittedMemoryMb = null;
        }
        finally
        {
            process.Dispose();
        }
    }

    private void TrackProcess(Process process)
    {
        if (_trackedProcessId == process.Id)
        {
            return;
        }

        _trackedProcessId = process.Id;
        _previousProcessorTime =
            process.TotalProcessorTime;

        _previousSampleTime =
            DateTime.UtcNow;
    }

    private void UpdateCpuUsage(
        Process process,
        TelemetryData telemetry)
    {
        DateTime currentSampleTime =
            DateTime.UtcNow;

        TimeSpan currentProcessorTime =
            process.TotalProcessorTime;

        double elapsedMilliseconds =
            (currentSampleTime - _previousSampleTime)
            .TotalMilliseconds;

        double processorMilliseconds =
            (currentProcessorTime - _previousProcessorTime)
            .TotalMilliseconds;

        if (elapsedMilliseconds > 0)
        {
            double cpuUsage =
                processorMilliseconds
                / elapsedMilliseconds
                / Environment.ProcessorCount
                * 100d;

            telemetry.CpuUsagePercent =
                Math.Clamp(cpuUsage, 0d, 100d);
        }

        _previousProcessorTime =
            currentProcessorTime;

        _previousSampleTime =
            currentSampleTime;
    }

    private static Process? GetStarCitizenProcess()
    {
        return Process
            .GetProcessesByName("StarCitizen")
            .FirstOrDefault();
    }

    private void Reset()
    {
        _trackedProcessId = null;
        _previousProcessorTime = TimeSpan.Zero;
        _previousSampleTime = default;
    }

    public void Dispose()
    {
        Reset();
    }
}