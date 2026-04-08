using Microsoft.VisualStudio.Extensibility.UI;

namespace VSClock.Dialogs;

internal class SettingsDialogControl(object? dataContext, SynchronizationContext? synchronizationContext = null)
    : RemoteUserControl(dataContext, synchronizationContext)
{
}
