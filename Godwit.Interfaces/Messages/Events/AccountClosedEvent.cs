using System;

namespace Godwit.Interfaces.Messages.Events {
    public class AccountClosedEvent : IEvent {
        public AccountClosedEvent(Guid accountId, string reason) {
            AccountId = accountId;
            Reason = reason;
            Id = Guid.NewGuid();
            TimeStamp = DateTimeOffset.Now;
        }

        public Guid Id { get; }
        public DateTimeOffset TimeStamp { get; }
        public Guid AccountId { get; }
        public string Reason { get; }
    }

}