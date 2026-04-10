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
    private TextBlock? _textBlock;

    [VisualStudioContribution]
    public static BrokeredServiceConfiguration BrokeredServiceConfiguration
        => new(IInProcService.Configuration.ServiceName, IInProcService.Configuration.ServiceVersion, typeof(InProcService))
        {
            ServiceAudience = BrokeredServiceAudience.Local | BrokeredServiceAudience.Public,
        };

    public async Task Inject(CancellationToken cancellationToken)
    {
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
    }

    public async Task UpdateClock(string format)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (_textBlock == null)
        {
            return;
        }

        _textBlock.Text = DateTime.Now.ToString(format);
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
