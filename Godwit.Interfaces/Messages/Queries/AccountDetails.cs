using System;

namespace Godwit.Interfaces.Messages.Queries {
    public class AccountDetails {
        public AccountDetails(Guid id, string status, decimal amount, string customerNumber, string branchCode,
            string closeReason, DateTimeOffset openTimeStamp, DateTimeOffset? closeTimeStamp) {
            Id = id;
            Status = status;
            Amount = amount;
            CustomerNumber = customerNumber;
            BranchCode = branchCode;
            CloseReason = closeReason;
            OpenTimeStamp = openTimeStamp;
            CloseTimeStamp = closeTimeStamp;
        }

        public Guid Id { get; }
        public string Status { get; }
        public decimal Amount { get; }
        public string CustomerNumber { get; }
        public string BranchCode { get; }
        public string CloseReason { get; }
        public DateTimeOffset OpenTimeStamp { get; }
        public DateTimeOffset? CloseTimeStamp { get; }
    }

    public class AccountInfo {
        public AccountInfo(Guid id, string status, DateTimeOffset openTimeStamp, DateTimeOffset? closeTimeStamp) {
            Id = id;
            Status = status;
            OpenTimeStamp = openTimeStamp;
            CloseTimeStamp = closeTimeStamp;
        }

        public Guid Id { get; }
        public string Status { get; }
        public DateTimeOffset OpenTimeStamp { get; }
        public DateTimeOffset? CloseTimeStamp { get; }
    }
}