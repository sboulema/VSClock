using Microsoft.ServiceHub.Framework;

namespace VSClock.Services;

public interface IInProcService
{
    Task Inject(CancellationToken cancellationToken);

    Task UpdateClock(string format, bool showClockIcon);

    public static class Configuration
    {
        public const string ServiceName = "VSClock.InProcService";
        public static readonly Version ServiceVersion = new(1, 0);

        public static readonly ServiceMoniker ServiceMoniker = new(ServiceName, ServiceVersion);

        public static ServiceRpcDescriptor ServiceDescriptor => new ServiceJsonRpcDescriptor(
            ServiceMoniker,
            ServiceJsonRpcDescriptor.Formatters.MessagePack,
            ServiceJsonRpcDescriptor.MessageDelimiters.BigEndianInt32LengthHeader);
    }
}
