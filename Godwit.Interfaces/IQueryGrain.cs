using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godwit.Interfaces.Messages.Queries;

namespace Godwit.Interfaces {
    public interface IQueryGrain {
        Task<AccountDetails> GetAccountDetails(Guid accountId);
        Task<IEnumerable<AccountInfo>> GetCustomerAccounts(string customerNumber);
        
    }
}