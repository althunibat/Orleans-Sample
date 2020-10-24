using System;
using Godwit.Interfaces.Messages.Events;

namespace Godwit.Grains.Domain {
    public class Transaction {
        public Transaction(AccountDepositedEvent @event) {
            Type = TransactionType.Credit;
            Amount = @event.Amount;
            TransactionId = @event.TransactionId;
        }

        public Transaction(AccountWithDrawnEvent @event) {
            Type = TransactionType.Debit;
            Amount = @event.Amount;
            TransactionId = @event.TransactionId;
        }

        public decimal Amount { get; }
        public TransactionType Type { get; }
        public Guid TransactionId { get; }
    }
}