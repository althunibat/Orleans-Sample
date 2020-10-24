using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.Client;
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
using Transaction = Godwit.Interfaces.Messages.Queries.Transaction;

namespace Godwit.Grains {
    public class AccountGrain : JournaledGrain<AccountState, IEvent>, IAccountGrain,
        ICustomStorageInterface<AccountState, IEvent> {
        private readonly EventStoreClient _eventStoreClient;
        private readonly ILogger _logger;
        private string _streamName;

        public AccountGrain(ILogger<AccountGrain> logger, EventStoreClient eventStoreClient) {
            this._logger = logger;
            _eventStoreClient = eventStoreClient;
        }

        private string StreamName {
            get {
                if (string.IsNullOrWhiteSpace(_streamName))
                    _streamName = string.Concat(nameof(AccountState).ToLower(), "-",
                        GrainReference.GetPrimaryKey().ToString("N"));
                return _streamName;
            }
        }

        public async Task<bool> OpenAccount(OpenAccountCommand cmd) {
            if (State.Status != AccountStatus.NotInitialized || cmd.Amount <= 0)
                throw new AccountTransactionException(
                    $"Invalid Operation Detected! the account state is {State.Status} or Amount value is invalid");

            var @event = new AccountOpenedEvent(GrainReference.GetPrimaryKey(), cmd.CustomerNumber, cmd.BranchCode,
                cmd.Amount);
            await RaiseEvent(@event);
            return true;
        }


        public async Task<bool> CloseAccount(CloseAccountCommand cmd) {
            if (State.Status != AccountStatus.Open && State.Amount >= 0)
                throw new AccountTransactionException(
                    $"Invalid Operation Detected! the account state is {State.Status}");

            var @event = new AccountClosedEvent(GrainReference.GetPrimaryKey(), cmd.Reason);
            await RaiseEvent(@event);
            return true;
        }

        public async Task<bool> DepositAccount(DepositAccountCommand cmd) {
            if (State.Status != AccountStatus.Open)
                throw new AccountTransactionException(
                    $"Invalid Operation Detected! the account {GrainReference.GetPrimaryKey():N} is not open!");
            // is same command is received, then just return.
            if (this.State[cmd.Id] != null)
                return true;
            var @event = new AccountDepositedEvent(GrainReference.GetPrimaryKey(), cmd.Amount, cmd.Id);
            await RaiseEvent(@event);
            return true;
        }

        public async Task<bool> WithDrawAccount(WithDrawAccountCommand cmd) {
            if (State.Status != AccountStatus.Open || State.Amount - cmd.Amount <= 0)
                throw new AccountTransactionException(
                    $"Invalid Operation Detected! the account {GrainReference.GetPrimaryKey():N} is not open! or invalid amount passed!");
            // is same command is received, then just return.
            if (this.State[cmd.Id] != null)
                return true;
            var @event = new AccountWithDrawnEvent(GrainReference.GetPrimaryKey(), cmd.Amount, cmd.Id);
            await RaiseEvent(@event);
            return true;
        }

        public async Task<bool> RevertTransaction(RevertTransaction cmd) {
            if (State.Status != AccountStatus.Open || State[cmd.TransactionId] == null)
                throw new AccountTransactionException(
                    $"Invalid Operation Detected! the account {GrainReference.GetPrimaryKey():N} is not open! Referenced Transaction is Invalid");
            var transaction = State[cmd.TransactionId];
            switch (transaction.Type) {
                case TransactionType.Credit:
                    var evt1 = new AccountDepositedEvent(GrainReference.GetPrimaryKey(), transaction.Amount, cmd.Id);
                    await RaiseEvent(evt1);
                    break;
                case TransactionType.Debit:
                    var evt2 = new AccountWithDrawnEvent(GrainReference.GetPrimaryKey(), transaction.Amount, cmd.Id);
                    await RaiseEvent(evt2);
                    break;
            }

            return true;
        }

        public Task<AccountDetails> GetAccountDetails() {
            if (State.Status == AccountStatus.NotInitialized) return Task.FromResult<AccountDetails>(null);

            var details = new AccountDetails(GrainReference.GetPrimaryKey(), State.Status.ToString(),
                State.Amount, State.CustomerNumber, State.BranchCode, State.CloseReason,
                State.OpenTimeStamp, State.CloseTimeStamp,
                State.Transactions.Select(t => new Transaction(t.TransactionId, t.Type.ToString(), t.Amount)).ToList());
            return Task.FromResult(details);
        }

        public async Task<KeyValuePair<int, AccountState>> ReadStateFromStorage() {
            _logger.LogInformation("Reading Events for Aggregate {0}", GrainReference.GetPrimaryKey().ToString("N"));
            var root = new AccountState();
            
            await foreach (var @event in LoadEvents()) root.Apply(@event);

            return new KeyValuePair<int, AccountState>(root.Version, root);
        }

        public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<IEvent> updates, int expectedVersion) {
            _logger.LogInformation("Applying Events for Aggregate {0}",
                GrainReference.GetPrimaryKey().ToString("N"));
            var version = await GetCurrentVersion();
            if (version != expectedVersion) {
                _logger.LogCritical("Expected version not matched for {0} ==> {1}!= {2}",
                    GrainReference.GetPrimaryKey().ToString("N"), version, expectedVersion);
                throw new AccountTransactionException(
                    $"Concurrency Exception Detected!");
            }
            await _eventStoreClient.AppendToStreamAsync(StreamName, StreamState.Any,
                updates.Select(@event=>new EventData(Uuid.FromGuid(@event.Id), @event.GetType().AssemblyQualifiedName??@event.GetType().Name,
                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)))));
            return true;
        }

        private async Task RaiseEvent(IEvent @event) {
            base.RaiseEvent(@event);
            await ConfirmEvents();
        }

        private async IAsyncEnumerable<IEvent> LoadEvents() {
            var events = _eventStoreClient.ReadStreamAsync(
                Direction.Forwards,
                StreamName,
                StreamPosition.Start);
            if (await events.ReadState == ReadState.StreamNotFound) yield break;
            await foreach (var @event in events) {
                yield return DeserializeEvent(@event.Event);
            }

        }

        private IEvent DeserializeEvent(EventRecord @event) {
            var eventType = Type.GetType(@event.EventType);
            if (eventType == null)
                _logger.LogCritical("Couldn't load type '{0}'. Are you missing an assembly reference?",
                    @event.EventType);

            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(@event.Data.ToArray()), eventType) as IEvent;
        }


        private async Task<long> GetCurrentVersion() {
            var result = _eventStoreClient.ReadStreamAsync(Direction.Backwards, StreamName, StreamPosition.End, 1);
            var state = await result.ReadState;
            if (state != ReadState.Ok) return 0;
            var e = await result.FirstAsync();
            return e.Event.EventNumber.ToInt64() + 1;
        }
    }
}