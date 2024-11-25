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
        private readonly SessionStore _sessionStore;

        public AccountController(AccountService accountService, SessionStore  sessionStore)
        {
            _accountService = accountService;
            _sessionStore = sessionStore;
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
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var user = await _accountService.ValidateUserAsync(loginRequest.Username, loginRequest.Password);

            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.UtcNow.AddHours(1);

            _sessionStore.AddSession(token, user.Id, expiry);

            return Ok(new { Token = token, Expiry = expiry });
        }



        [HttpGet("logout")]
        public IActionResult Logout([FromHeader(Name = "Authorization")] string token)
        {
            _sessionStore.RemoveSession(token); // Use the correct method from SessionStore
            return Ok("Logged out successfully.");
        }


        [HttpGet("me")]
        public IActionResult Me([FromHeader(Name = "Authorization")] string authorization)
        {
            // Check if Authorization header is provided
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return Unauthorized(new { message = "Authorization token is required." });
            }

            var token = authorization.Substring("Bearer ".Length).Trim();

            // Retrieve the session using the token
            if (_sessionStore.TryGetSession(token, out var session))
            {
                if (session.Expiry >= DateTime.UtcNow) 
                {
                    return Ok(new
                    {
                        UserId = session.UserId,
                        Token = token,
                        Expiry = session.Expiry
                    });
                }
                else
                {
                    return Unauthorized(new { message = "Session token has expired." });
                }
            }
            else
            {
                return Unauthorized(new { message = "Invalid session token." });
            }
        }

    }
}