using System;
using Godwit.Interfaces.Messages.Events;

namespace Godwit.Grains.Domain {
    [Serializable]
    public class AccountState {
        public AccountState() {
            Status = AccountStatus.NotInitialized;
            Amount = 0;
            Version = 0;
        }

        public int Version { get; private set; }
        public AccountStatus Status { get; private set; }
        public decimal Amount { get; private set; }
        public string CustomerNumber { get; private set; }
        public string BranchCode { get; private set; }
        public string CloseReason { get; private set; }
        public DateTimeOffset OpenTimeStamp { get; private set; }
        public DateTimeOffset? CloseTimeStamp { get; private set; }

        public void Apply(IEvent @event) {
            switch (@event) {
                case IAccountOpened evt1:
                    Status = AccountStatus.Open;
                    Amount = evt1.Amount;
                    BranchCode = evt1.BranchCode;
                    CustomerNumber = evt1.CustomerNumber;
                    OpenTimeStamp = evt1.TimeStamp;
                    Version++;
                    break;
                case IAccountClosed evt2:
                    Status = AccountStatus.Closed;
                    CloseReason = evt2.Reason;
                    CloseTimeStamp = evt2.TimeStamp;
                    Version++;
                    break;
                case IAccountDeposited evt3:
                    Amount += evt3.Amount;
                    break;
                case IAccountWithDrawn evt4:
                    Amount -= evt4.Amount;
                    Version++;
                    break;
            }
        }
    }
}