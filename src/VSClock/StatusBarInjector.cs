using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;

namespace VSClock;

internal static class StatusBarInjector
{
    private static Panel? _statusbarPanel;

    // Constants for better maintainability
    private const string StatusBarPanelName = "StatusBarPanel";
    private const int StatusBarRetryDelayMilliseconds = 5000;

    private static DependencyObject? FindChild(DependencyObject? parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
            {
                return frameworkElement;
            }

            child = FindChild(child, childName);

            if (child != null)
            {
                return child;
            }
        }

        return null;
    }

    private static async Task EnsureUIAsync()
    {
        while (_statusbarPanel is null)
        {
            _statusbarPanel = FindChild(Application.Current?.MainWindow, StatusBarPanelName) as DockPanel;

            if (_statusbarPanel is null)
            {
                // Start window is showing. Need to wait for status bar render.
                await Task.Delay(StatusBarRetryDelayMilliseconds);
            }
        }
    }

    public static async Task InjectControlAsync(FrameworkElement? element)
    {
        if (element == null)
        {
            return;
        }

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        await EnsureUIAsync();

        element.SetValue(DockPanel.DockProperty, Dock.Right);

        _statusbarPanel?.Children.Insert(1, element);
    }

    public static async Task MoveToLast(FrameworkElement? element)
    {
        if (element == null ||
            _statusbarPanel == null)
        {
            return;
        }

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var currentIndex = _statusbarPanel.Children.IndexOf(element);

        if (currentIndex == -1 ||
            currentIndex == 1)
        {
            return;
        }

        _statusbarPanel.Children.Remove(element);
        _statusbarPanel.Children.Insert(1, element);
    }
}