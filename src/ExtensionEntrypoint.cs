using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;
using VSClock.Services;

namespace VSClock;

[VisualStudioContribution]
internal class ExtensionEntrypoint : Extension
{
    private ClockService? _clockService;

    /// <inheritdoc />
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        RequiresInProcessHosting = true,
        LoadedWhen = ActivationConstraint.SolutionState(SolutionState.NoSolution),
    };

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);

        // As of now, any instance that ingests VisualStudioExtensibility is required to be added as a scoped
        // service.
        serviceCollection.AddScoped<ClockService>();
    }

    protected override async Task OnInitializedAsync(VisualStudioExtensibility extensibility, CancellationToken cancellationToken)
    {
        await base.OnInitializedAsync(extensibility, cancellationToken);

        _clockService = ServiceProvider.GetRequiredService<ClockService>();
    }
}
