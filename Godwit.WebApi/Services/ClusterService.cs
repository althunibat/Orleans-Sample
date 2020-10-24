using System;
using System.Threading;
using System.Threading.Tasks;
using Godwit.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;

namespace Godwit.WebApi.Services {
    public class ClusterService : IHostedService {
        private readonly ILogger<ClusterService> _logger;

        public ClusterService(ILogger<ClusterService> logger) {
            this._logger = logger;

            Client = new ClientBuilder()
                .ConfigureApplicationParts(manager =>
                    manager.AddApplicationPart(typeof(IAccountGrain).Assembly).WithReferences())
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options => {
                    options.ClusterId = "dev";
                    options.ServiceId = "HelloWorldApp";
                })
                .Build();
        }

        public IClusterClient Client { get; }

        public async Task StartAsync(CancellationToken cancellationToken) {
            await Client.Connect(async error => {
                _logger.LogError(error, error.Message);
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                return true;
            });
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Client.Close();
        }
    }
}