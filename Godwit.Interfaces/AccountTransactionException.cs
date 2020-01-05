using System;
using System.Runtime.Serialization;

namespace Godwit.Interfaces {
    public class AccountTransactionException : ApplicationException {
        public AccountTransactionException() {
        }

        public AccountTransactionException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        public AccountTransactionException(string message) : base(message) {
        }

        public AccountTransactionException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}