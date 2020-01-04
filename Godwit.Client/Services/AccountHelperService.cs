using System;
using System.Threading;
using System.Threading.Tasks;
using Godwit.Interfaces;
using Godwit.Interfaces.Messages.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Godwit.Client.Services {
    public class AccountHelperService : IHostedService {
        private readonly IClusterClient _client;
        private readonly ILogger _logger;

        public AccountHelperService(IClusterClient client, ILogger<AccountHelperService> logger) {
            this._client = client;
            this._logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            var acc1 = Guid.NewGuid();
            var client1 = _client.GetGrain<IAccountGrain>(acc1);
            var details = await client1.GetAccountDetails();
            if (details == null)
                _logger.LogInformation(
                    $"Account: {acc1} doesn't exist!");
            else
                _logger.LogInformation(
                    $"Account: {details.Id} opened on {details.OpenTimeStamp} and its status {details.Status}. It has Amount {details.Amount}");

            await client1.OpenAccount(GetOpenAccountCommand());
            details = await client1.GetAccountDetails();
            _logger.LogInformation(
                $"Account: {details.Id} opened on {details.OpenTimeStamp} and its status {details.Status}. It has Amount {details.Amount}");
            await client1.DepositAccount(new DepositAccountCommand(100));
            details = await client1.GetAccountDetails();
            _logger.LogInformation(
                $"Account: {details.Id} opened on {details.OpenTimeStamp} and its status {details.Status}. It has Amount {details.Amount}");
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        private static OpenAccountCommand GetOpenAccountCommand() {
            return new OpenAccountCommand("1234", "0567", 1000);
        }

        private static DepositAccountCommand GetDepositAccountCommand() {
            return new DepositAccountCommand(200);
        }
    }
}