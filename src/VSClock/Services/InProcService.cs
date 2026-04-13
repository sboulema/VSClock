using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VSClock.OutOfProc.Services;

namespace VSClock.Services;

[VisualStudioContribution]
internal class InProcService(VisualStudioExtensibility extensibility) : IInProcService
{
    private DockPanel? _clockDockPanel;
    private TextBlock? _textBlock;

    [VisualStudioContribution]
    public static BrokeredServiceConfiguration BrokeredServiceConfiguration
        => new(IInProcService.Configuration.ServiceName, IInProcService.Configuration.ServiceVersion, typeof(InProcService))
        {
            ServiceAudience = BrokeredServiceAudience.Local | BrokeredServiceAudience.Public,
        };

    /// <summary>
    /// Create, wire up events, and inject the clock control into the status bar.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Awaitable Task</returns>
    public async Task Inject(CancellationToken cancellationToken)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        _clockDockPanel = new DockPanel
        {
            Margin = new Thickness(0, 0, 5, 0),
            Height = 22,
        };

        _clockDockPanel.MouseUp += OpenSettingsDialog;

        AddTimeIcon(_clockDockPanel);

        AddTextBlock(_clockDockPanel);

        await StatusBarInjector.InjectControlAsync(_clockDockPanel);
    }

    /// <summary>
    /// Update the date time display on the clock and ensure the clock is the last item in the status bar.
    /// </summary>
    /// <param name="format">DateTime format</param>
    /// <returns>Awaitable Task</returns>
    public async Task UpdateClock(string format)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (_textBlock != null)
        {
            _textBlock.Text = DateTime.Now.ToString(format);
        }

        await StatusBarInjector.MoveToLast(_clockDockPanel);
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

    private void OpenSettingsDialog(object sender, MouseButtonEventArgs e)
        => _ = OpenSettingsDialog();

    private async Task OpenSettingsDialog(CancellationToken cancellationToken = default)
    {
        var outOfProcService = await extensibility.ServiceBroker
            .GetProxyAsync<IOutOfProcService>(IOutOfProcService.Configuration.ServiceDescriptor, cancellationToken: default);

        try
        {
            Assumes.NotNull(outOfProcService);

            await outOfProcService.OpenSettingsDialog(cancellationToken);
        }
        finally
        {
            (outOfProcService as IDisposable)?.Dispose();
        }
    }
}
