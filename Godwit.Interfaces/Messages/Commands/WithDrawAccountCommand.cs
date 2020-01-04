using System;

namespace Godwit.Interfaces.Messages.Commands {
    public class WithDrawAccountCommand : ICommand {
        public WithDrawAccountCommand(decimal amount) {
            Amount = amount;
            Id = Guid.NewGuid();
            TimeStamp = DateTimeOffset.Now;
        }

        public decimal Amount { get; }
        public Guid Id { get; }
        public DateTimeOffset TimeStamp { get; }
    }
}