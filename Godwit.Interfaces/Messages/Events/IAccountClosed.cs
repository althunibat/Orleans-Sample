namespace Godwit.Interfaces.Messages.Events {
    public interface IAccountClosed : IEvent {
        string Reason { get; }
    }
}