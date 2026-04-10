using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Helpers;
using VSClock.OutOfProc.Helpers;
using VSClock.Services;
using Timer = System.Timers.Timer;

namespace VSClock.OutOfProc.Services;

internal class ClockService(VisualStudioExtensibility extensibility) : DisposableObject
{
    private IInProcService? _inProcService;

    public Timer? UpdateTimer;

    public string Format = GetDefaultFormat();
    public int UpdateInterval = 1000;

    public async Task InitializeAsync()
    {
        await InitializeSettings();

        InitializeTimer();

        (_inProcService as IDisposable)?.Dispose();
        _inProcService = await extensibility
            .ServiceBroker
            .GetProxyAsync<IInProcService>(IInProcService.Configuration.ServiceDescriptor, cancellationToken: default);

        await InjectClock(default);
    }

    private async Task InitializeSettings()
    {
        var globalSettings = await SettingsHelper.LoadGlobalSettings();
        Format = globalSettings.Format;
        UpdateInterval = globalSettings.UpdateInterval;
    }

    private void InitializeTimer()
    {
        UpdateTimer = new Timer(UpdateInterval);
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

            await _inProcService.Inject(cancellationToken);
        }
        catch (Exception)
        {
            // TODO: Add logging
        }
    }

    private async Task UpdateClock()
    {
        try
        {
            Assumes.NotNull(_inProcService);
            await _inProcService.UpdateClock(Format);
        }
        catch (Exception)
        {
            // TODO: Add logging
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
