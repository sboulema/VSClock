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

        serviceCollection.AddSingleton<ClockService>();
    }

    protected override Task OnInitializedAsync(VisualStudioExtensibility extensibility, CancellationToken cancellationToken)
    {
        _clockService = ServiceProvider.GetRequiredService<ClockService>();

        return Task.CompletedTask;
    }
}
