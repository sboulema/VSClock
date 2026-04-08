using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;
using VSClock.Services;

namespace VSClock;

[VisualStudioContribution]
internal class ExtensionEntrypoint : Extension
{
    private readonly ClockService _clockService = new();

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
    }

    protected override async Task OnInitializedAsync(VisualStudioExtensibility extensibility, CancellationToken cancellationToken)
    {
        await _clockService.Initialize(extensibility);
    }
}
