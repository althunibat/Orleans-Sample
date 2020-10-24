using System;
using System.ComponentModel.DataAnnotations;
using Godwit.Interfaces.Messages.Commands;

namespace Godwit.WebApi.Requests {
    public class OpenAccount {
        [Required] public string CustomerNumber { get; set; }
        [Required] public string BranchCode { get; set; }

        [Required, Range(0, double.PositiveInfinity)]
        public decimal Amount { get; set; }

        public OpenAccountCommand ToCommand() {
            return new OpenAccountCommand(CustomerNumber, BranchCode, Amount);
        }
    }
}