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
    private TextBlock? _clockTextBlock;
    private CrispImage? _clockIcon;

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

        CreateClockDockPanel();

        CreateClockTextBlock();

        CreateClockIcon();

        InsertElement(_clockTextBlock);

        await StatusBarInjector.InjectControlAsync(_clockDockPanel);
    }

    /// <summary>
    /// Update the date time display on the clock and ensure the clock is the last item in the status bar.
    /// </summary>
    /// <param name="format">DateTime format</param>
    /// <returns>Awaitable Task</returns>
    public async Task UpdateClock(string format, bool showClockIcon)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (_clockTextBlock != null)
        {
            _clockTextBlock.Text = DateTime.Now.ToString(format);
        }

        if (showClockIcon)
        {
            InsertElement(_clockIcon);
        }
        else
        {
            RemoveClockIcon();
        }

        if (_clockTextBlock != null)
        {
            _clockTextBlock.Margin = new Thickness(showClockIcon ? 0 : 9, 0, 9, 0);
        }

        await StatusBarInjector.MoveToLast(_clockDockPanel);
    }

    private void RemoveClockIcon()
    {
        if (_clockDockPanel == null ||
            !_clockDockPanel.Children.Contains(_clockIcon))
        {
            return;
        }

        _clockDockPanel.Children.Remove(_clockIcon);
    }

    /// <summary>
    /// Create and wire up the DockPanel that holds the other elements 
    /// </summary>
    /// <remarks>Creation is done in a method since we need to be on the UI thread.</remarks>
    private void CreateClockDockPanel()
    {
        _clockDockPanel = new()
        {
            Height = 22,
        };

        _clockDockPanel.MouseUp += OpenSettingsDialog;

        _clockDockPanel.MouseEnter += ChangePanelBackground;
        _clockDockPanel.MouseLeave += ChangePanelBackground;
    }

    private void ChangePanelBackground(object sender, MouseEventArgs e)
    {
        if (Application.Current.TryFindResource(VsBrushes.StartPageTextSubHeadingSelectedKey) is not SolidColorBrush brush)
        {
            return;
        }

        _clockDockPanel!.Background = _clockDockPanel.IsMouseOver
            ? new SolidColorBrush(brush.Color) { Opacity = 0.2 }
            : Brushes.Transparent;
    }

    /// <summary>
    /// Create the textblock that will show the date and time 
    /// </summary>
    /// <remarks>Creation is done in a method since we need to be on the UI thread.</remarks>
    private void CreateClockTextBlock()
    {
        _clockTextBlock = new()
        {
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Application.Current.TryFindResource(VsBrushes.StatusBarTextKey) as SolidColorBrush,
            Margin = new Thickness(0, 0, 9, 0),
        };
    }

    /// <summary>
    /// Create the clock icon that will show 
    /// </summary>
    /// <remarks>Creation is done in a method since we need to be on the UI thread.</remarks>
    private void CreateClockIcon()
    {
        _clockIcon = new()
        {
            Moniker = KnownMonikers.Time,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(9, 0, 9, 0),
        };
    }

    /// <summary>
    /// Insert framework element to the VSClock DockPanel in the status bar if it's not already added.
    /// </summary>
    private void InsertElement(FrameworkElement? frameworkElement)
    {
        if (frameworkElement == null ||
            _clockDockPanel == null ||
            _clockDockPanel.Children.Contains(frameworkElement))
        {
            return;
        }

        _clockDockPanel.Children.Insert(0, frameworkElement);
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
