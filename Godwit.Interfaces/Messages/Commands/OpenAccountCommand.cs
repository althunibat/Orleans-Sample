using System;

namespace Godwit.Interfaces.Messages.Commands {
    public class OpenAccountCommand : ICommand {
        public OpenAccountCommand(string customerNumber, string branchCode, decimal amount) {
            CustomerNumber = customerNumber;
            BranchCode = branchCode;
            Amount = amount;
            Id = Guid.NewGuid();
            TimeStamp = DateTimeOffset.Now;
        }

        public string CustomerNumber { get; }
        public string BranchCode { get; }
        public decimal Amount { get; }

        public Guid Id { get; }
        public DateTimeOffset TimeStamp { get; }
    }
}