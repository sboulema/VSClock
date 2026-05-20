using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Helpers;
using VSClock.OutOfProc.Helpers;
using VSClock.Services;
using Timer = System.Timers.Timer;

namespace VSClock.OutOfProc.Services;

internal class ClockService(
    VisualStudioExtensibility extensibility,
    OutputWindowService outputWindowService) : DisposableObject
{
    private IInProcService? _inProcService;
    private Timer? UpdateTimer;

    public async Task InitializeAsync()
    {
        var globalSettings = await SettingsHelper.LoadGlobalSettings(outputWindowService);

        InitializeTimer(globalSettings.UpdateInterval);

        (_inProcService as IDisposable)?.Dispose();
        _inProcService = await extensibility
            .ServiceBroker
            .GetProxyAsync<IInProcService>(IInProcService.Configuration.ServiceDescriptor, cancellationToken: default);

        await InjectClock(default);
    }

    private void InitializeTimer(int interval)
    {
        UpdateTimer = new Timer(interval);
        UpdateTimer.Elapsed += OnUpdateTick;
        UpdateTimer.Start();
    }

    private void OnUpdateTick(object? sender, EventArgs e)
        => _ = UpdateClock();

    private async Task InjectClock(CancellationToken cancellationToken)
    {
        try
        {
            Assumes.NotNull(_inProcService);

            var result = await _inProcService.Inject(cancellationToken);

            if (!string.IsNullOrEmpty(result))
            {
                await outputWindowService.WriteException("Failed to inject clock.", new Exception(result));
            }
        }
        catch (Exception e)
        {
            await outputWindowService.WriteException("Failed to inject clock.", e);
        }
    }

    private async Task UpdateClock()
    {
        try
        {
            Assumes.NotNull(_inProcService);

            var globalSettings = await SettingsHelper.GetGlobalSettings(outputWindowService);

            var result = await _inProcService.UpdateClock(globalSettings.Format, globalSettings.ShowClockIcon);

            if (!string.IsNullOrEmpty(result))
            {
                await outputWindowService.WriteException("Failed to update clock.", new Exception(result));
            }
        }
        catch (Exception e)
        {
            await outputWindowService.WriteException("Failed to update clock.", e);
        }
    }

    public static string GetDefaultFormat()
    {
        var dateTimeFormat = Thread.CurrentThread.CurrentCulture.DateTimeFormat;

        if (dateTimeFormat == null)
        {
            return "yyyy-MM-dd (dddd) HH:mm:ss"; ;
        }

        return $"{dateTimeFormat.ShortTimePattern} {dateTimeFormat.ShortDatePattern}";
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        if (isDisposing)
        {
            (_inProcService as IDisposable)?.Dispose();
        }
    }
}
