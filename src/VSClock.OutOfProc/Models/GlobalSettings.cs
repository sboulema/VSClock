using VSClock.OutOfProc.Services;

namespace VSClock.OutOfProc.Models;

public class GlobalSettings
{
    /// <summary>
    /// Format string for the clock display.
    /// </summary>
    public string Format { get; set; } = ClockService.GetDefaultFormat();

    /// <summary>
    /// Update interval for the clock in milliseconds. Default is 1000ms (1 second).
    /// </summary>
    public int UpdateInterval { get; set; } = 1000;

    /// <summary>
    /// Show clock icon in the status bar. Default is true.
    /// </summary>
    public bool ShowClockIcon { get; set; } = true;
}
