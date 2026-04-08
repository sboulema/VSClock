using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VSClock.Helpers;
using VSClock.Models;

namespace VSClock.Services;

internal class ClockService()
{
    private DispatcherTimer? _updateTimer;
    private TextBlock? _textBlock;
    private VisualStudioExtensibility? _extensibility;

    private string _format = "yyyy-MM-dd (dddd) HH:mm:ss";
    private int _updateInterval = 1000;

    public async Task Initialize(VisualStudioExtensibility extensibility)
    {
        _extensibility = extensibility;

        await InitializeSettings();

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var panel = new DockPanel
        {
            Margin = new Thickness(0, 0, 5, 0),
            Height = 22,
        };

        panel.MouseUp += PromptSettings;

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

    private void PromptSettings(object sender, MouseButtonEventArgs e)
        => _ = PromptSettings();

    private async Task PromptSettings()
    {
        if (_extensibility == null)
        {
            return;
        }

        var formatPromptString = await _extensibility.Shell().ShowPromptAsync(
            "Format",
            InputPromptOptions.Question,
            default);

        var updateIntervalPromptString = await _extensibility.Shell().ShowPromptAsync(
            "Update Interval",
            InputPromptOptions.Question,
            default);

        var updateInterval = int.TryParse(updateIntervalPromptString, out var interval) ? interval : _updateInterval;

        var globalSettings = new GlobalSettings
        {
            Format = !string.IsNullOrEmpty(formatPromptString) ? formatPromptString! : _format,
            UpdateInterval = updateInterval
        };

        await SettingsHelper.SaveGlobalSettings(globalSettings);

        _format = globalSettings.Format;

        if (_updateTimer != null)
        {
            _updateTimer.Interval = TimeSpan.FromMilliseconds(globalSettings.UpdateInterval);
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
