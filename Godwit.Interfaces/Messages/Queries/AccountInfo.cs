using System;

namespace Godwit.Interfaces.Messages.Queries {
    public class AccountInfo {
        public AccountInfo(Guid id, string status) {
            Id = id;
            Status = status;
        }

        public Guid Id { get; }
        public string Status { get; }
    }
}