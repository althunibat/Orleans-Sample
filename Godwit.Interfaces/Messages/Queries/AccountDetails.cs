using System;
using System.Collections.Generic;

namespace Godwit.Interfaces.Messages.Queries {
    public class AccountDetails {
        public AccountDetails(Guid id, string status, decimal amount, string customerNumber, string branchCode,
            string closeReason, DateTimeOffset openTimeStamp, DateTimeOffset? closeTimeStamp,
            IEnumerable<Transaction> transactions) {
            Id = id;
            Status = status;
            Amount = amount;
            CustomerNumber = customerNumber;
            BranchCode = branchCode;
            CloseReason = closeReason;
            OpenTimeStamp = openTimeStamp;
            CloseTimeStamp = closeTimeStamp;
            Transactions = transactions;
        }

        public Guid Id { get; }
        public string Status { get; }
        public decimal Amount { get; }
        public string CustomerNumber { get; }
        public string BranchCode { get; }
        public string CloseReason { get; }
        public DateTimeOffset OpenTimeStamp { get; }
        public DateTimeOffset? CloseTimeStamp { get; }
        public IEnumerable<Transaction> Transactions { get; }
    }

    public class Transaction {
        public Transaction(Guid id, string type, decimal amount) {
            Type = type;
            Amount = amount;
            Id = id;
        }

        public Guid Id { get; }
        public string Type { get; }
        public decimal Amount { get; }
    }
}