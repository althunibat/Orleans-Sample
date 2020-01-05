using System;

namespace Godwit.Interfaces.Messages.Events {
    public class AccountWithDrawnEvent : IEvent {
        public AccountWithDrawnEvent(Guid accountId, decimal amount) {
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