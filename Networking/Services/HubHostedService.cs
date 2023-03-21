using Microsoft.Extensions.Hosting;

namespace Networking.Services
{
    public class HubHostedService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => {
                HubService.Init();
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                HubService.Close();
            }, cancellationToken);
        }
    }
}
