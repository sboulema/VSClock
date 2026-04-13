using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;
using VSClock.OutOfProc.Services;

namespace VSClock.OutOfProc;

/// <summary>
/// Extension entrypoint for the VisualStudio.Extensibility extension.
/// </summary>
[VisualStudioContribution]
internal class ExtensionEntrypoint : Extension
{
    /// <inheritdoc/>
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
                id: "VSClock.07a4b650-aeca-474d-9cd6-4f5d58691f4b",
                version: ExtensionAssemblyVersion,
                publisherName: "Samir Boulema",
                displayName: "VSClock",
                description: "Show a clock in the Visual Studio statusbar"),
    };

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);

        // Add the brokered service for in-out proc communication
        serviceCollection.ProfferBrokeredService<OutOfProcService>();

        // As of now, any instance that ingests VisualStudioExtensibility is required to be added as a scoped
        // service.
        serviceCollection.AddScoped<ClockService>();
    }
}
