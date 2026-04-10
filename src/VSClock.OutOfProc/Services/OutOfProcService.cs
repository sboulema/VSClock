using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.RpcContracts.Notifications;
using VSClock.OutOfProc.Dialogs;
using VSClock.OutOfProc.Helpers;

namespace VSClock.OutOfProc.Services;

[VisualStudioContribution]
internal class OutOfProcService(
    VisualStudioExtensibility extensibility,
    ClockService clockService) : IOutOfProcService, IBrokeredService
{
    public static BrokeredServiceConfiguration BrokeredServiceConfiguration
        => new(IOutOfProcService.Configuration.ServiceName, IOutOfProcService.Configuration.ServiceVersion, typeof(OutOfProcService))
        {
            ServiceAudience = BrokeredServiceAudience.Local | BrokeredServiceAudience.Public,
        };

    public static ServiceRpcDescriptor ServiceDescriptor => IOutOfProcService.Configuration.ServiceDescriptor;

    public async Task StartClock(CancellationToken cancellationToken)
    {
        await clockService.InitializeAsync();
    }

    public async Task OpenSettingsDialog(CancellationToken cancellationToken)
    {
        var globalSettings = await SettingsHelper.LoadGlobalSettings();

        var settingsDialogData = new SettingsDialogData
        {
            Format = globalSettings.Format,
            UpdateInterval = globalSettings.UpdateInterval
        };

        var dialogResult = await extensibility.Shell().ShowDialogAsync(
            content: new SettingsDialogControl(settingsDialogData),
            title: "VS Clock Settings",
            options: new(DialogButton.OKCancel, DialogResult.OK),
            cancellationToken);

        if (dialogResult == DialogResult.Cancel)
        {
            return;
        }

        // Save settings to disk
        await SettingsHelper.SaveGlobalSettings(new()
        {
            Format = settingsDialogData.Format,
            UpdateInterval = settingsDialogData.UpdateInterval
        });

        // Apply the new format (on the next tick) on the clock display
        clockService.Format = settingsDialogData.Format;

        // Apply the new interval on the update timer
        if (clockService.UpdateTimer != null)
        {
            clockService.UpdateTimer.Interval = settingsDialogData.UpdateInterval;
        }
    }
}
