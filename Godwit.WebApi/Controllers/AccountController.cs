using System;
using System.Threading.Tasks;
using Godwit.Interfaces;
using Godwit.Interfaces.Messages.Commands;
using Godwit.WebApi.Requests;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace Godwit.WebApi.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase {
        private readonly IClusterClient _client;

        public AccountController(IClusterClient client) {
            _client = client;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute]Guid id) {
           var accProxy = _client.GetGrain<IAccountGrain>(id);
           var details = await accProxy.GetAccountDetails();
           if (details == null)
               return NotFound();
           return Ok(details);
        }
        
        [HttpPost("open")]
        public async Task<IActionResult> Open(OpenAccount request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var cmd = request.ToCommand();
            var accProxy = _client.GetGrain<IAccountGrain>(cmd.Id);
            try {
                await accProxy.OpenAccount(cmd);
            }
            catch (AccountTransactionException e) {
                ModelState.AddModelError("InvalidOperation",e.Message);
                return BadRequest(ModelState);
            }
            return Ok(new {AccountId = cmd.Id});
        }
        
        [HttpDelete("{accountId}")]
        public async Task<IActionResult> Close([FromRoute] Guid accountId, CloseAccount request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var cmd = request.ToCommand();
            var accProxy = _client.GetGrain<IAccountGrain>(accountId);
            try {
                await accProxy.CloseAccount(cmd);
            }
            catch (AccountTransactionException e) {
                ModelState.AddModelError("InvalidOperation",e.Message);
                return BadRequest(ModelState);
            }
            return Ok();
        }
        
        [HttpPut("{accountId}/deposit/{amount}")]
        public async Task<IActionResult> Deposit([FromRoute] Guid accountId,decimal amount) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var cmd = new DepositAccountCommand(amount);
            var accProxy = _client.GetGrain<IAccountGrain>(accountId);
            try {
                await accProxy.DepositAccount(cmd);
            }
            catch (AccountTransactionException e) {
                ModelState.AddModelError("InvalidOperation",e.Message);
                return BadRequest(ModelState);
            }
            return NoContent();
        }
        
        [HttpPut("{accountId}/withdraw/{amount}")]
        public async Task<IActionResult> Withdraw([FromRoute] Guid accountId,decimal amount) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var cmd = new WithDrawAccountCommand(amount);
            var accProxy = _client.GetGrain<IAccountGrain>(accountId);
            try {
                await accProxy.WithDrawAccount(cmd);
            }
            catch (AccountTransactionException e) {
                ModelState.AddModelError("InvalidOperation",e.Message);
                return BadRequest(ModelState);
            }
            return NoContent();
        }
        
        
    }
}