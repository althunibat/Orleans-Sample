namespace Godwit.Interfaces.Messages.Events {
    public interface IAccountOpened : IEvent {
        string CustomerNumber { get; }
        string BranchCode { get; }
        decimal Amount { get; }
    }
}