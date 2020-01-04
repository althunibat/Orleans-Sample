using System;

namespace Godwit.Interfaces.Messages.Commands {
    public class CloseAccountCommand : ICommand {
        public CloseAccountCommand(string reason) {
            Reason = reason;
            Id = Guid.NewGuid();
            TimeStamp = DateTimeOffset.Now;
        }

        public string Reason { get; }
        public Guid Id { get; }
        public DateTimeOffset TimeStamp { get; }
    }
}