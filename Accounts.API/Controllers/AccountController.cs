using Microsoft.AspNetCore.Mvc;
using Accounts.API.DTO;
using Accounts.API.Services;

namespace Accounts.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }
        
        public class CreateUserResponse
        {
            public Guid UserId { get; set; }
        }
        
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterUserRequest registerUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = await _accountService.CreateUserAsync(registerUser);

            return Ok(new CreateUserResponse { UserId = userId });
        }
    }
}