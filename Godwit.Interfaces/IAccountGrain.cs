using System.Threading.Tasks;
using Godwit.Interfaces.Messages.Commands;
using Godwit.Interfaces.Messages.Queries;
using Orleans;

namespace Godwit.Interfaces {
    public interface IAccountGrain : IGrainWithGuidKey {
        public Task<bool> OpenAccount(OpenAccountCommand cmd);
        public Task<bool> CloseAccount(CloseAccountCommand cmd);
        public Task<bool> DepositAccount(DepositAccountCommand cmd);
        public Task<bool> WithDrawAccount(WithDrawAccountCommand cmd);
        public Task<bool> RevertTransaction(RevertTransaction cmd);

        public Task<AccountDetails> GetAccountDetails();
    }
}