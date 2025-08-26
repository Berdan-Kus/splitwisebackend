using SplitwiseAPI.DTOs.UserDTOs;
using SplitwiseAPI.DTOs.UserExpenseDTOs;
using SplitwiseAPI.Models;
using SplitwiseAPI.Repositories.Interfaces;
using SplitwiseAPI.Services.Interfaces;

namespace SplitwiseAPI.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserExpenseRepository _userExpenseRepository;

        public UserService(IUserRepository userRepository, IUserExpenseRepository userExpenseRepository)
        {
            _userRepository = userRepository;
            _userExpenseRepository = userExpenseRepository;
        }

        // Basic CRUD operations
        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? MapToResponseDto(user) : null;
        }

        public async Task<UserResponseDto?> GetUserByPhoneAsync(string phone)
        {
            var user = await _userRepository.GetByPhoneAsync(phone);
            return user != null ? MapToResponseDto(user) : null;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToResponseDto);
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Validate phone uniqueness
            if (await _userRepository.PhoneExistsAsync(createUserDto.Phone))
            {
                throw new InvalidOperationException("Phone number already exists");
            }

            var user = new User
            {
                Name = createUserDto.Name,
                Phone = createUserDto.Phone,
                Password = HashPassword(createUserDto.Password) // In real app, use proper password hashing
            };

            var createdUser = await _userRepository.CreateAsync(user);
            return MapToResponseDto(createdUser);
        }

        public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateUserDto.Name))
                user.Name = updateUserDto.Name;

            if (!string.IsNullOrEmpty(updateUserDto.Phone))
            {
                if (await _userRepository.PhoneExistsAsync(updateUserDto.Phone, id))
                {
                    throw new InvalidOperationException("Phone number already exists");
                }
                user.Phone = updateUserDto.Phone;
            }

            if (!string.IsNullOrEmpty(updateUserDto.Password))
                user.Password = HashPassword(updateUserDto.Password);

            var updatedUser = await _userRepository.UpdateAsync(user);
            return MapToResponseDto(updatedUser);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        // User validation
        public async Task<bool> UserExistsAsync(int id)
        {
            return await _userRepository.ExistsAsync(id);
        }

        public async Task<bool> PhoneExistsAsync(string phone, int? excludeUserId = null)
        {
            return await _userRepository.PhoneExistsAsync(phone, excludeUserId);
        }

        public async Task<bool> ValidateUserCredentialsAsync(string phone, string password)
        {
            var user = await _userRepository.GetByPhoneAsync(phone);
            return user != null && VerifyPassword(password, user.Password);
        }

        // User relationships
        public async Task<IEnumerable<UserResponseDto>> GetUsersByGroupIdAsync(int groupId)
        {
            var users = await _userRepository.GetUsersByGroupIdAsync(groupId);
            return users.Select(MapToResponseDto);
        }

        public async Task<UserResponseDto?> GetUserWithGroupsAsync(int userId)
        {
            var user = await _userRepository.GetUserWithGroupsAsync(userId);
            return user != null ? MapToResponseDtoWithGroups(user) : null;
        }

        public async Task<UserResponseDto?> GetUserWithExpensesAsync(int userId)
        {
            var user = await _userRepository.GetUserWithExpensesAsync(userId);
            return user != null ? MapToResponseDtoWithExpenses(user) : null;
        }

        public async Task<UserResponseDto?> GetUserWithFullDetailsAsync(int userId)
        {
            var user = await _userRepository.GetUserWithFullDetailsAsync(userId);
            return user != null ? MapToResponseDtoWithFullDetails(user) : null;
        }

        // User balance and debt operations
        public async Task<UserBalanceDto> GetUserBalanceAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new ArgumentException("User not found");

            try
            {
                // Simplified version to avoid collection enumeration issues
                var totalPaid = await _userRepository.GetUserTotalPaidAsync(userId);
                var totalOwed = await _userRepository.GetUserTotalOwedAsync(userId);
                var netBalance = totalPaid - totalOwed;

                return new UserBalanceDto
                {
                    UserId = userId,
                    UserName = user.Name,
                    TotalPaid = totalPaid,
                    TotalOwed = totalOwed,
                    NetBalance = netBalance,
                    DebtDetails = new List<UserDebtDetailDto>() // Simplified - empty list
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error calculating user balance: {ex.Message}");
            }
        }

        public async Task<UserDashboardDto> GetUserDashboardAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new ArgumentException("User not found");

            try
            {
                var netBalance = await _userRepository.GetUserNetBalanceAsync(userId);

                return new UserDashboardDto
                {
                    UserId = userId,
                    UserName = user.Name,
                    TotalNetBalance = netBalance,
                    ActiveGroupsCount = 0, // Simplified
                    TotalExpensesCount = 0, // Simplified
                    RecentActivities = new List<RecentActivityDto>(),
                    ActiveDebts = new List<ActiveDebtDto>()
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating dashboard: {ex.Message}");
            }
        }

        public async Task<IEnumerable<UserDebtDetailDto>> GetUserDebtsAsync(int userId)
        {
            var balanceWithOthers = await _userExpenseRepository.GetUserBalanceWithOthersAsync(userId);
            return await BuildDebtDetailsAsync(userId, balanceWithOthers.Where(b => b.Value < 0).ToDictionary(b => b.Key, b => b.Value));
        }

        public async Task<IEnumerable<UserDebtDetailDto>> GetUserCreditsAsync(int userId)
        {
            var balanceWithOthers = await _userExpenseRepository.GetUserBalanceWithOthersAsync(userId);
            return await BuildDebtDetailsAsync(userId, balanceWithOthers.Where(b => b.Value > 0).ToDictionary(b => b.Key, b => b.Value));
        }

        // Search operations
        public async Task<IEnumerable<UserResponseDto>> SearchUsersByNameAsync(string name)
        {
            var users = await _userRepository.SearchUsersByNameAsync(name);
            return users.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<UserResponseDto>> SearchUsersByPhoneAsync(string phone)
        {
            var users = await _userRepository.SearchUsersByPhoneAsync(phone);
            return users.Select(MapToResponseDto);
        }

        // User statistics
        public async Task<decimal> GetUserTotalPaidAsync(int userId)
        {
            return await _userRepository.GetUserTotalPaidAsync(userId);
        }

        public async Task<decimal> GetUserTotalOwedAsync(int userId)
        {
            return await _userRepository.GetUserTotalOwedAsync(userId);
        }

        public async Task<decimal> GetUserNetBalanceAsync(int userId)
        {
            return await _userRepository.GetUserNetBalanceAsync(userId);
        }

        public async Task<int> GetUserActiveGroupsCountAsync(int userId)
        {
            var userGroups = await _userRepository.GetUserWithGroupsAsync(userId);
            return userGroups?.UserGroups?.Count ?? 0;
        }

        // Private helper methods
        private static UserResponseDto MapToResponseDto(User user)
        {
            return new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Phone = user.Phone
            };
        }

        private static UserResponseDto MapToResponseDtoWithGroups(User user)
        {
            var dto = MapToResponseDto(user);
            dto.Groups = user.UserGroups?.Select(ug => new UserGroupResponseDto
            {
                GroupId = ug.Group.GroupId,
                GroupName = ug.Group.Name
            }).ToList();
            return dto;
        }

        private static UserResponseDto MapToResponseDtoWithExpenses(User user)
        {
            var dto = MapToResponseDto(user);
            dto.Expenses = user.UserExpenses?.Select(ue => new UserExpenseDetailDto
            {
                ExpenseId = ue.Expense.ExpenseId,
                Description = ue.Expense.Description,
                Amount = ue.Amount,
                Type = ue.Type.ToString()
            }).ToList();
            return dto;
        }

        private static UserResponseDto MapToResponseDtoWithFullDetails(User user)
        {
            var dto = MapToResponseDtoWithGroups(user);
            dto.Expenses = user.UserExpenses?.Select(ue => new UserExpenseDetailDto
            {
                ExpenseId = ue.Expense.ExpenseId,
                Description = ue.Expense.Description,
                Amount = ue.Amount,
                Type = ue.Type.ToString()
            }).ToList();
            return dto;
        }

        private async Task<List<UserDebtDetailDto>> BuildDebtDetailsAsync(int userId, Dictionary<int, decimal> balanceWithOthers)
        {
            var debtDetails = new List<UserDebtDetailDto>();

            foreach (var balance in balanceWithOthers)
            {
                var otherUser = await _userRepository.GetByIdAsync(balance.Key);
                if (otherUser != null)
                {
                    debtDetails.Add(new UserDebtDetailDto
                    {
                        OtherUserId = balance.Key,
                        OtherUserName = otherUser.Name,
                        Amount = Math.Abs(balance.Value),
                        Type = balance.Value > 0 ? "OWED" : "OWE",
                        RelatedExpenses = new List<DebtExpenseDto>() // Will be populated if needed
                    });
                }
            }

            return debtDetails;
        }

        private static string HashPassword(string password)
        {
            // In production, use BCrypt or similar
            return password; // Simplified for demo
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            // In production, use proper password verification
            return password == hashedPassword; // Simplified for demo
        }
    }
}