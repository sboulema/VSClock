using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.RpcContracts.Notifications;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VSClock.Dialogs;
using VSClock.Helpers;

namespace VSClock.Services;

internal class ClockService
{
    private readonly VisualStudioExtensibility _extensibility;
    private readonly Task _initializationTask;

    private DispatcherTimer? _updateTimer;
    private TextBlock? _textBlock;

    private string _format = "yyyy-MM-dd (dddd) HH:mm:ss";
    private int _updateInterval = 1000;

    public ClockService(VisualStudioExtensibility extensibility)
    {
        _extensibility = extensibility;
        _initializationTask = Task.Run(InitializeAsync);
    }

    public async Task InitializeAsync()
    {
        await InitializeSettings();

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var panel = new DockPanel
        {
            Margin = new Thickness(0, 0, 5, 0),
            Height = 22,
        };

        panel.MouseUp += OpenSettingsDialog;

        AddTimeIcon(panel);

        AddTextBlock(panel);

        await StatusBarInjector.InjectControlAsync(panel);

        InitializeTimer();
    }

    private async Task InitializeSettings()
    {
        var globalSettings = await SettingsHelper.LoadGlobalSettings();
        _format = globalSettings.Format;
        _updateInterval = globalSettings.UpdateInterval;
    }

    private void InitializeTimer()
    {
        _updateTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(_updateInterval)
        };
        _updateTimer.Tick += OnUpdateTick;
        _updateTimer.Start();
    }

    private CrispImage AddTimeIcon(DockPanel panel)
    {
        var timeIcon = new CrispImage
        {
            Moniker = KnownMonikers.Time,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 5, 0),
        };

        panel.Children.Add(timeIcon);

        return timeIcon;
    }

    private void AddTextBlock(DockPanel panel)
    {
        var brush = Application.Current.TryFindResource(VsBrushes.StatusBarTextKey) as SolidColorBrush;

        _textBlock = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = brush
        };

        panel.Children.Add(_textBlock);
    }

    private void OnUpdateTick(object? sender, EventArgs e)
        => _ = UpdateClock();

    private void OpenSettingsDialog(object sender, MouseButtonEventArgs e)
        => OpenSettingsDialog().Wait();

    private async Task OpenSettingsDialog(CancellationToken cancellationToken = default)
    {
        if (_extensibility == null)
        {
            return;
        }

        var settingsDialogData = new SettingsDialogData
        {
            Format = _format,
            UpdateInterval = _updateInterval
        };

        var dialogResult = await _extensibility.Shell().ShowDialogAsync(
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
        _format = settingsDialogData.Format;

        // Apply the new interval on the update timer
        if (_updateTimer != null)
        {
            _updateTimer.Interval = TimeSpan.FromMilliseconds(settingsDialogData.UpdateInterval);
        }
    }

    private async Task UpdateClock()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (_textBlock == null)
        {
            return;
        }

        _textBlock.Text = DateTime.Now.ToString(_format);
    }
}
