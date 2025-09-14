using Microsoft.EntityFrameworkCore;
using SplitwiseAPI.Data;
using SplitwiseAPI.Models;
using SplitwiseAPI.Repositories.Interfaces;

namespace SplitwiseAPI.Repositories.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<User?> GetByPhoneAsync(string phone)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Phone == phone);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(u => u.UserId == id);
        }

        public async Task<bool> PhoneExistsAsync(string phone, int? excludeUserId = null)
        {
            var query = _context.Users.Where(u => u.Phone == phone);
            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.UserId != excludeUserId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByGroupIdAsync(int groupId)
        {
            return await _context.Users
                .Include(u => u.UserGroups)
                .Where(u => u.UserGroups.Any(ug => ug.GroupId == groupId))
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<int> userIds)
        {
            return await _context.Users
                .Where(u => userIds.Contains(u.UserId))
                .ToListAsync();
        }

        public async Task<User?> GetUserWithGroupsAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.UserGroups)
                    .ThenInclude(ug => ug.Group)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetUserWithExpensesAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.UserExpenses)
                    .ThenInclude(ue => ue.Expense)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetUserWithFullDetailsAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.UserGroups)
                    .ThenInclude(ug => ug.Group)
                .Include(u => u.UserExpenses)
                    .ThenInclude(ue => ue.Expense)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<decimal> GetUserTotalPaidAsync(int userId)
        {
            return await _context.UserExpenses
                .Where(ue => ue.UserId == userId && ue.Type == UserExpenseType.PAID_BY)
                .SumAsync(ue => ue.Amount);
        }

        public async Task<decimal> GetUserTotalOwedAsync(int userId)
        {
            return await _context.UserExpenses
                .Where(ue => ue.UserId == userId && ue.Type == UserExpenseType.HEAD_TO_PAY)
                .SumAsync(ue => ue.Amount);
        }

        public async Task<decimal> GetUserNetBalanceAsync(int userId)
        {
            var totalPaid = await GetUserTotalPaidAsync(userId);
            var totalOwed = await GetUserTotalOwedAsync(userId);
            return totalPaid - totalOwed;
        }

        public async Task<IEnumerable<User>> GetUsersWithDebtToUserAsync(int creditorUserId)
        {
            var debtorIds = await _context.UserExpenses
                .Include(ue => ue.Expense)
                .Where(ue => ue.Expense.UserExpenses.Any(ue2 =>
                    ue2.UserId == creditorUserId && ue2.Type == UserExpenseType.PAID_BY))
                .Where(ue => ue.Type == UserExpenseType.HEAD_TO_PAY && ue.UserId != creditorUserId)
                .Select(ue => ue.UserId)
                .Distinct()
                .ToListAsync();

            return await GetUsersByIdsAsync(debtorIds);
        }

        public async Task<IEnumerable<User>> GetUsersUserOwesAsync(int debtorUserId)
        {
            var creditorIds = await _context.UserExpenses
                .Include(ue => ue.Expense)
                .Where(ue => ue.UserId == debtorUserId && ue.Type == UserExpenseType.HEAD_TO_PAY)
                .SelectMany(ue => ue.Expense.UserExpenses
                    .Where(ue2 => ue2.Type == UserExpenseType.PAID_BY && ue2.UserId != debtorUserId)
                    .Select(ue2 => ue2.UserId))
                .Distinct()
                .ToListAsync();

            return await GetUsersByIdsAsync(creditorIds);
        }

        public async Task<IEnumerable<User>> SearchUsersByNameAsync(string name)
        {
            return await _context.Users
                .Where(u => u.Name.Contains(name))
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> SearchUsersByPhoneAsync(string phone)
        {
            return await _context.Users
                .Where(u => u.Phone.Contains(phone))
                .ToListAsync();
        }
    }
}