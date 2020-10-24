using System;

namespace Godwit.Interfaces.Messages {
    public interface IMessage {
        public Guid Id { get; }
        public DateTimeOffset TimeStamp { get; }
    }
}