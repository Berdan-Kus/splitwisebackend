using Microsoft.AspNetCore.Mvc;
using SplitwiseAPI.DTOs.UserDTOs;
using SplitwiseAPI.DTOs.UserExpenseDTOs;
using SplitwiseAPI.Services.Interfaces;

namespace SplitwiseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), 200)]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost]
        [ProducesResponseType(typeof(UserResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                var user = await _userService.CreateUserAsync(createUserDto);
                return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound($"User with ID {id} not found");

            return Ok(user);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(id, updateUserDto);
                if (user == null)
                    return NotFound($"User with ID {id} not found");

                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/balance")]
        [ProducesResponseType(typeof(UserBalanceDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserBalanceDto>> GetUserBalance(int id)
        {
            try
            {
                var balance = await _userService.GetUserBalanceAsync(id);
                return Ok(balance);
            }
            catch (ArgumentException)
            {
                return NotFound($"User with ID {id} not found");
            }
        }

        [HttpGet("{id}/dashboard")]
        [ProducesResponseType(typeof(UserDashboardDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserDashboardDto>> GetUserDashboard(int id)
        {
            try
            {
                var dashboard = await _userService.GetUserDashboardAsync(id);
                return Ok(dashboard);
            }
            catch (ArgumentException)
            {
                return NotFound($"User with ID {id} not found");
            }
        }

        [HttpGet("search/phone/{phone}")]
        [ProducesResponseType(typeof(UserResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserResponseDto>> SearchUserByPhone(string phone)
        {
            var user = await _userService.GetUserByPhoneAsync(phone);
            if (user == null)
                return NotFound($"User with phone {phone} not found");

            return Ok(user);
        }

        [HttpGet("phone-exists/{phone}")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> CheckPhoneExists(string phone, [FromQuery] int? excludeUserId = null)
        {
            var exists = await _userService.PhoneExistsAsync(phone, excludeUserId);
            return Ok(exists);
        }

        [HttpPost("validate")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> ValidateCredentials([FromBody] ValidateCredentialsDto validateCredentialsDto)
        {
            var isValid = await _userService.ValidateUserCredentialsAsync(validateCredentialsDto.Phone, validateCredentialsDto.Password);
            return Ok(isValid);
        }
    }

    public class ValidateCredentialsDto
    {
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}