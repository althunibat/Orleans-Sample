using System;

namespace Godwit.Interfaces.Messages.Commands {
    public class RevertTransaction:ICommand {
        public RevertTransaction(Guid transactionId) {
            TransactionId = transactionId;
            Id = Guid.NewGuid();
            TimeStamp = DateTimeOffset.UtcNow;
        }

        public Guid TransactionId { get; }
        public Guid Id { get; }
        public DateTimeOffset TimeStamp { get; }
    }
}