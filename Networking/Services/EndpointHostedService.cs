using Microsoft.Extensions.Hosting;

namespace Networking.Services
{
    public class EndpointHostedService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => {
                EndpointService.Init();
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                EndpointService.Close();
            }, cancellationToken);
        }
    }
}
