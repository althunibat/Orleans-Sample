using System.Collections.Generic;
using System.Threading.Tasks;
using Godwit.Interfaces.Messages.Queries;

namespace Godwit.Interfaces {
    public interface IQueryGrain {
        Task<AccountDetails> GetAccountDetails();
        Task<IEnumerable<AccountInfo>> GetAccountsList(string customerNumber);
    }
}