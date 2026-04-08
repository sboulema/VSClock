using Microsoft.VisualStudio.Extensibility.VSSdkCompatibility;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSClock.Services;

internal class ClockService
{
    private readonly AsyncServiceProviderInjection<SVsStatusbar, IVsStatusbar> _vsStatusbar;
    private readonly System.Timers.Timer? _timer = new();

    private string _format = "yyyy-MM-dd (dddd) HH:mm:ss";
    private int _updateInterval = 1000;

    public ClockService(
        AsyncServiceProviderInjection<SVsStatusbar,
        IVsStatusbar> vsStatusbar)
    {
        _vsStatusbar = vsStatusbar;
        InitializeTimer();
    }

    private void InitializeTimer()
    {
        _timer!.Interval = _updateInterval;
        _timer.Elapsed += Timer_Elapsed;
        _timer.Enabled = true;
    }

    private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        => _ = UpdateClock();

    private async Task UpdateClock()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var vsStatusbarService = await _vsStatusbar.GetServiceAsync();

        try
        {
            vsStatusbarService.SetText(DateTime.Now.ToString(_format));

            vsStatusbarService.IsFrozen(out int frozen);

            if (frozen != 0)
            {
                vsStatusbarService.FreezeOutput(0);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"!!! EXCEPTION in UpdateClock: {ex.Message}");
        }
    }
}
