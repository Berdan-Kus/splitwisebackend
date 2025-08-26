using SplitwiseAPI.DTOs.UserDTOs;
using SplitwiseAPI.DTOs.UserExpenseDTOs;

namespace SplitwiseAPI.Services.Interfaces
{
    public interface IUserService
    {
        // Basic CRUD operations
        Task<UserResponseDto?> GetUserByIdAsync(int id);
        Task<UserResponseDto?> GetUserByPhoneAsync(string phone);
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);

        // User validation
        Task<bool> UserExistsAsync(int id);
        Task<bool> PhoneExistsAsync(string phone, int? excludeUserId = null);
        Task<bool> ValidateUserCredentialsAsync(string phone, string password);

        // User relationships
        Task<IEnumerable<UserResponseDto>> GetUsersByGroupIdAsync(int groupId);
        Task<UserResponseDto?> GetUserWithGroupsAsync(int userId);
        Task<UserResponseDto?> GetUserWithExpensesAsync(int userId);
        Task<UserResponseDto?> GetUserWithFullDetailsAsync(int userId);

        // User balance and debt operations
        Task<UserBalanceDto> GetUserBalanceAsync(int userId);
        Task<UserDashboardDto> GetUserDashboardAsync(int userId);
        Task<IEnumerable<UserDebtDetailDto>> GetUserDebtsAsync(int userId);
        Task<IEnumerable<UserDebtDetailDto>> GetUserCreditsAsync(int userId);

        // Search operations
        Task<IEnumerable<UserResponseDto>> SearchUsersByNameAsync(string name);
        Task<IEnumerable<UserResponseDto>> SearchUsersByPhoneAsync(string phone);

        // User statistics
        Task<decimal> GetUserTotalPaidAsync(int userId);
        Task<decimal> GetUserTotalOwedAsync(int userId);
        Task<decimal> GetUserNetBalanceAsync(int userId);
        Task<int> GetUserActiveGroupsCountAsync(int userId);
    }
}