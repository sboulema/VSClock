using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.UI;
using System.Diagnostics;
using System.Runtime.Serialization;
using VSClock.OutOfProc.Services;

namespace VSClock.OutOfProc.Dialogs;

[DataContract]
public class SettingsDialogData
{
    public SettingsDialogData()
    {
        OpenHyperlinkCommand = new(OpenHyperlink);
    }

    /// <summary>
    /// Format string for the clock display.
    /// </summary>
    [DataMember]
    public string Format { get; set; } = ClockService.GetDefaultFormat();

    /// <summary>
    /// Update interval for the clock in milliseconds. Default is 1000ms (1 second).
    /// </summary>
    [DataMember]
    public int UpdateInterval { get; set; } = 1000;

    /// <summary>
    /// Show clock icon in the status bar. Default is true.
    /// </summary>
    [DataMember]
    public bool ShowClockIcon { get; set; } = true;

    [DataMember]
    public AsyncCommand OpenHyperlinkCommand { get; }
    private async Task OpenHyperlink(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        if (commandParameter == null)
        {
            return;
        }

        Process.Start(new ProcessStartInfo { FileName = commandParameter.ToString(), UseShellExecute = true });
    }
}
