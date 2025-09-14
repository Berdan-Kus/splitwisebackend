using SplitwiseAPI.Models;

namespace SplitwiseAPI.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByPhoneAsync(string phone);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> PhoneExistsAsync(string phone, int? excludeUserId = null);

        Task<IEnumerable<User>> GetUsersByGroupIdAsync(int groupId);
        Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<int> userIds);
        Task<User?> GetUserWithGroupsAsync(int userId);
        Task<User?> GetUserWithExpensesAsync(int userId);
        Task<User?> GetUserWithFullDetailsAsync(int userId);

        Task<decimal> GetUserTotalPaidAsync(int userId);
        Task<decimal> GetUserTotalOwedAsync(int userId);
        Task<decimal> GetUserNetBalanceAsync(int userId);
        Task<IEnumerable<User>> GetUsersWithDebtToUserAsync(int creditorUserId);
        Task<IEnumerable<User>> GetUsersUserOwesAsync(int debtorUserId);

        Task<IEnumerable<User>> SearchUsersByNameAsync(string name);
        Task<IEnumerable<User>> SearchUsersByPhoneAsync(string phone);
    }
}