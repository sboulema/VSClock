namespace VSClock.Models;

public class GlobalSettings
{
    /// <summary>
    /// Format string for the clock display. Default is "yyyy-MM-dd (dddd) HH:mm:ss".
    /// </summary>
    public string Format { get; set; } = "yyyy-MM-dd (dddd) HH:mm:ss";

    /// <summary>
    /// Update interval for the clock in milliseconds. Default is 1000ms (1 second).
    /// </summary>
    public int UpdateInterval { get; set; } = 1000;
}
