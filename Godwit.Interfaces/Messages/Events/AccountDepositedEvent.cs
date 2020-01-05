using System;

namespace Godwit.Interfaces.Messages.Events {
    public class AccountDepositedEvent : IEvent {
        public AccountDepositedEvent(Guid accountId, decimal amount) {
            AccountId = accountId;
            Amount = amount;
            Id = Guid.NewGuid();
            TimeStamp = DateTimeOffset.Now;
        }

        public Guid Id { get; }
        public DateTimeOffset TimeStamp { get; }
        public Guid AccountId { get; }
        public decimal Amount { get; }
    }

}