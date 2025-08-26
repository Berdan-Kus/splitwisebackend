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

        /// <summary>
        /// Get all users (for dropdowns, member selection)
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), 200)]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="createUserDto">User creation data</param>
        /// <returns>Created user</returns>
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

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
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

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="updateUserDto">User update data</param>
        /// <returns>Updated user</returns>
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

        /// <summary>
        /// Get user balance (core feature)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User balance information</returns>
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

        /// <summary>
        /// Get user dashboard (core feature)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User dashboard data</returns>
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

        /// <summary>
        /// Search users by phone number
        /// </summary>
        /// <param name="phone">Phone number</param>
        /// <returns>User details</returns>
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

        /// <summary>
        /// Check if phone number exists (for registration validation)
        /// </summary>
        /// <param name="phone">Phone number</param>
        /// <param name="excludeUserId">User ID to exclude from check</param>
        /// <returns>Existence status</returns>
        [HttpGet("phone-exists/{phone}")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> CheckPhoneExists(string phone, [FromQuery] int? excludeUserId = null)
        {
            var exists = await _userService.PhoneExistsAsync(phone, excludeUserId);
            return Ok(exists);
        }

        /// <summary>
        /// Validate user credentials (for login)
        /// </summary>
        /// <param name="validateCredentialsDto">Credentials to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> ValidateCredentials([FromBody] ValidateCredentialsDto validateCredentialsDto)
        {
            var isValid = await _userService.ValidateUserCredentialsAsync(validateCredentialsDto.Phone, validateCredentialsDto.Password);
            return Ok(isValid);
        }
    }

    // Additional DTOs for controller endpoints
    public class ValidateCredentialsDto
    {
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}