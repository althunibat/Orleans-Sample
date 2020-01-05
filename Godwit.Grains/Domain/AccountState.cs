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
                case AccountOpenedEvent evt1:
                    Apply(evt1);
                    break;
                case AccountClosedEvent evt2:
                   Apply(evt2);
                    break;
                case AccountDepositedEvent evt3:
                   Apply(evt3);
                    break;
                case AccountWithDrawnEvent evt4:
                   Apply(evt4);
                    break;
            }
        }

        private void Apply(AccountOpenedEvent @event) {
            Status = AccountStatus.Open;
            Amount = @event.Amount;
            BranchCode = @event.BranchCode;
            CustomerNumber = @event.CustomerNumber;
            OpenTimeStamp = @event.TimeStamp;
            Version++;
        }

        private void Apply(AccountClosedEvent @event) {
            Status = AccountStatus.Closed;
            CloseReason = @event.Reason;
            CloseTimeStamp = @event.TimeStamp;
            Version++;
        }

        private void Apply(AccountDepositedEvent @event) {
            Amount +=  @event.Amount;
            Version++;
        }

        private void Apply(AccountWithDrawnEvent @event) {
            Amount -= @event.Amount;
            Version++;
        }
    }
}