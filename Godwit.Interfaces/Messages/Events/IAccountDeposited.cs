namespace Godwit.Interfaces.Messages.Events {
    public interface IAccountDeposited : IEvent {
        decimal Amount { get; }
    }
}