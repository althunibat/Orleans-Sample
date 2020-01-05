using System.ComponentModel.DataAnnotations;
using Godwit.Interfaces.Messages.Commands;

namespace Godwit.WebApi.Requests {
    public class CloseAccount {
        [Required]
        public string Reason { get; set; }

        public CloseAccountCommand ToCommand() {
            return new CloseAccountCommand(Reason);
        }
    }
}