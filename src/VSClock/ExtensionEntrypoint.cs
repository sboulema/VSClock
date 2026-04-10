using Microsoft;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;
using VSClock.OutOfProc.Services;
using VSClock.Services;

namespace VSClock;

[VisualStudioContribution]
internal class ExtensionEntrypoint : Extension
{
    /// <inheritdoc />
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        RequiresInProcessHosting = true,
        LoadedWhen = ActivationConstraint.SolutionState(SolutionState.NoSolution),
    };

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.ProfferBrokeredService(InProcService.BrokeredServiceConfiguration, IInProcService.Configuration.ServiceDescriptor);

        base.InitializeServices(serviceCollection);
    }

    protected override async Task OnInitializedAsync(VisualStudioExtensibility extensibility, CancellationToken cancellationToken)
    {
        await base.OnInitializedAsync(extensibility, cancellationToken);

        await StartClock(extensibility, cancellationToken);
    }

    private async Task StartClock(VisualStudioExtensibility extensibility, CancellationToken cancellationToken)
    {
        var outOfProcService = await extensibility.ServiceBroker
            .GetProxyAsync<IOutOfProcService>(IOutOfProcService.Configuration.ServiceDescriptor, cancellationToken: default);

        try
        {
            Assumes.NotNull(outOfProcService);

            await outOfProcService.StartClock(cancellationToken);
        }
        finally
        {
            (outOfProcService as IDisposable)?.Dispose();
        }
    }
}
