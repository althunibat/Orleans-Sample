using System;

namespace Godwit.Interfaces.Messages.Events {
    public class AccountOpenedEvent : IEvent {
        public AccountOpenedEvent(Guid accountId, string customerNumber, string branchCode, decimal amount) {
            Id = Guid.NewGuid();
            TimeStamp = DateTimeOffset.Now;
            AccountId = accountId;
            CustomerNumber = customerNumber;
            BranchCode = branchCode;
            Amount = amount;
        }

        public Guid Id { get; }
        public DateTimeOffset TimeStamp { get; }
        public Guid AccountId { get; }
        public string CustomerNumber { get; }
        public string BranchCode { get; }
        public decimal Amount { get; }
    }
    
    
}