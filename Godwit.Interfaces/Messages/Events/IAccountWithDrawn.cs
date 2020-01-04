namespace Godwit.Interfaces.Messages.Events {
    public interface IAccountWithDrawn : IEvent {
        decimal Amount { get; }
    }
}