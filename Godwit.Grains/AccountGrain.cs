using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Godwit.Grains.Domain;
using Godwit.Interfaces;
using Godwit.Interfaces.Messages.Commands;
using Godwit.Interfaces.Messages.Events;
using Godwit.Interfaces.Messages.Queries;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Godwit.Grains {
    public class AccountGrain : JournaledGrain<AccountState, IEvent>, IAccountGrain,
        ICustomStorageInterface<AccountState, IEvent> {
        private readonly IEventStoreConnection _connection;
        private readonly ILogger _logger;

        public AccountGrain(ILogger<AccountGrain> logger, IEventStoreConnection connection) {
            this._logger = logger;
            this._connection = connection;
        }

        public async Task<bool> OpenAccount(OpenAccountCommand cmd) {
            if (State.Status != AccountStatus.NotInitialized || cmd.Amount <= 0) return false;

            var @event = new AccountOpenedEvent(GrainReference.GetPrimaryKey(), cmd.CustomerNumber, cmd.BranchCode,
                cmd.Amount);
            await RaiseEvent(@event);
            return true;
        }


        public async Task<bool> CloseAccount(CloseAccountCommand cmd) {
            if (State.Status != AccountStatus.Open && State.Amount >= 0) return false;

            var @event = new AccountClosedEvent(GrainReference.GetPrimaryKey(), cmd.Reason);
            await RaiseEvent(@event);
            return true;
        }

        public async Task<bool> DepositAccount(DepositAccountCommand cmd) {
            if (State.Status != AccountStatus.Open) return false;

            var @event = new AccountDepositedEvent(GrainReference.GetPrimaryKey(), cmd.Amount);
            await RaiseEvent(@event);
            return true;
        }

        public async Task<bool> WithDrawAccount(WithDrawAccountCommand cmd) {
            if (State.Status != AccountStatus.Open || State.Amount - cmd.Amount < 0) return false;

            var @event = new AccountWithDrawnEvent(GrainReference.GetPrimaryKey(), cmd.Amount);
            await RaiseEvent(@event);
            return true;
        }

        public Task<AccountDetails> GetAccountDetails() {
            if (State.Status == AccountStatus.NotInitialized) return Task.FromResult<AccountDetails>(null);

            var details = new AccountDetails(GrainReference.GetPrimaryKey(), State.Status.ToString(),
                State.Amount, State.CustomerNumber, State.BranchCode, State.CloseReason,
                State.OpenTimeStamp, State.CloseTimeStamp);
            return Task.FromResult(details);
        }

        public async Task<KeyValuePair<int, AccountState>> ReadStateFromStorage() {
            _logger.LogInformation("Reading Events for Aggregate {0}", this.GetPrimaryKeyString());
            var root = new AccountState();
            foreach (var @event in await LoadEvents(500)) root.Apply(@event);

            return new KeyValuePair<int, AccountState>(root.Version, root);
        }

        public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<IEvent> updates, int expectedVersion) {
            _logger.LogInformation("Applying Events for Aggregate {0}",
                GrainReference.GetPrimaryKey().ToString("N"));
            var version = await GetCurrentVersion();
            if (version != expectedVersion) {
                _logger.LogCritical("Expected version not matched for {0} ==> {1}!= {2}",
                    GrainReference.GetPrimaryKey().ToString("N"), version, expectedVersion);
                return false;
            }

            foreach (var @event in updates)
                await _connection.AppendToStreamAsync(GetStreamName(), ExpectedVersion.Any,
                    new EventData(@event.Id, @event.GetType().FullName, true,
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ArrayList()))));

            return true;
        }

        private async Task RaiseEvent(IEvent @event) {
            base.RaiseEvent(@event);
            await ConfirmEvents();
        }

        private async Task<ICollection<IEvent>> LoadEvents(int bufferSize) {
            var stream = GetStreamName();
            var streamEvents = new List<ResolvedEvent>();

            StreamEventsSlice currentSlice;
            long nextSliceStart = StreamPosition.Start;
            do {
                currentSlice =
                    await _connection.ReadStreamEventsForwardAsync(stream, nextSliceStart,
                        bufferSize, false);
                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);
            } while (!currentSlice.IsEndOfStream);

            return streamEvents.Select(e => DeserializeEvent(e.Event)).ToList();
        }

        private IEvent DeserializeEvent(RecordedEvent @event) {
            var eventType = Type.GetType(@event.EventType);
            if (eventType == null)
                _logger.LogCritical("Couldn't load type '{0}'. Are you missing an assembly reference?",
                    @event.EventType);

            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(@event.Data), eventType) as IEvent;
        }

        private string GetStreamName() {
            return string.Concat(typeof(AccountState).Name.ToLower(), "-",
                GrainReference.GetPrimaryKey().ToString("N"));
        }

        private async Task<int> GetCurrentVersion() {
            var stream = GetStreamName();
            var result = await _connection.ReadEventAsync(stream, StreamPosition.End, false);
            switch (result.Status) {
                case EventReadStatus.NoStream:
                case EventReadStatus.NotFound:
                    return 0;
                case EventReadStatus.Success:
                    return result.Event.HasValue ? (int) (result.Event.Value.Event.EventNumber + 1) : -1;
                default:
                    return -1;
            }
        }

        private class AccountOpenedEvent : IAccountOpened {
            public AccountOpenedEvent(Guid accountId, string customerNumber, string branchCode, decimal amount) {
                Id = Guid.NewGuid();
                TimeStamp = DateTimeOffset.Now;
                AccountId = accountId;
                CustomerNumber = customerNumber;
                BranchCode = branchCode;
                Amount = amount;
            }

            public Guid Id { get; }
            public DateTimeOffset TimeStamp { get; }
            public Guid AccountId { get; }
            public string CustomerNumber { get; }
            public string BranchCode { get; }
            public decimal Amount { get; }
        }

        private class AccountClosedEvent : IAccountClosed {
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

        private class AccountDepositedEvent : IAccountDeposited {
            public AccountDepositedEvent(Guid accountId, decimal amount) {
                AccountId = accountId;
                Amount = amount;
                Id = Guid.NewGuid();
                TimeStamp = DateTimeOffset.Now;
            }

            public Guid Id { get; }
            public DateTimeOffset TimeStamp { get; }
            public Guid AccountId { get; }
            public decimal Amount { get; }
        }

        private class AccountWithDrawnEvent : IAccountWithDrawn {
            public AccountWithDrawnEvent(Guid accountId, decimal amount) {
                AccountId = accountId;
                Amount = amount;
                Id = Guid.NewGuid();
                TimeStamp = DateTimeOffset.Now;
            }

            public Guid Id { get; }
            public DateTimeOffset TimeStamp { get; }
            public Guid AccountId { get; }
            public decimal Amount { get; }
        }
    }
}