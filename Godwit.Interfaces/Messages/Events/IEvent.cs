using System;

namespace Godwit.Interfaces.Messages.Events {
    public interface IEvent : IMessage {
        public Guid AccountId { get; }
    }
}