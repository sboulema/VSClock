using Microsoft.ServiceHub.Framework;

namespace VSClock.OutOfProc.Services;

public interface IOutOfProcService
{
    Task StartClock(CancellationToken cancellationToken);

    Task OpenSettingsDialog(CancellationToken cancellationToken);

    public static class Configuration
    {
        public const string ServiceName = "VSClock.OutOfProcService";
        public static readonly Version ServiceVersion = new(1, 0);

        public static readonly ServiceMoniker ServiceMoniker = new(ServiceName, ServiceVersion);

        public static ServiceRpcDescriptor ServiceDescriptor => new ServiceJsonRpcDescriptor(
            ServiceMoniker,
            ServiceJsonRpcDescriptor.Formatters.MessagePack,
            ServiceJsonRpcDescriptor.MessageDelimiters.BigEndianInt32LengthHeader);
    }
}
