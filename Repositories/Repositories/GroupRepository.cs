using Microsoft.EntityFrameworkCore;
using SplitwiseAPI.Data;
using SplitwiseAPI.Models;
using SplitwiseAPI.Repositories.Interfaces;

namespace SplitwiseAPI.Repositories.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly AppDbContext _context;

        public GroupRepository(AppDbContext context)
        {
            _context = context;
        }

        // Basic CRUD operations
        public async Task<Group?> GetByIdAsync(int id)
        {
            return await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == id);
        }

        public async Task<IEnumerable<Group>> GetAllAsync()
        {
            return await _context.Groups.ToListAsync();
        }

        public async Task<Group> CreateAsync(Group group)
        {
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            return group;
        }

        public async Task<Group> UpdateAsync(Group group)
        {
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();
            return group;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var group = await GetByIdAsync(id);
            if (group == null) return false;

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Groups.AnyAsync(g => g.GroupId == id);
        }

        // Group-specific operations
        public async Task<Group?> GetGroupWithMembersAsync(int groupId)
        {
            return await _context.Groups
                .Include(g => g.UserGroups)
                    .ThenInclude(ug => ug.User)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);
        }

        public async Task<Group?> GetGroupWithExpensesAsync(int groupId)
        {
            return await _context.Groups
                .Include(g => g.Expenses)
                    .ThenInclude(e => e.UserExpenses)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);
        }

        public async Task<Group?> GetGroupWithFullDetailsAsync(int groupId)
        {
            return await _context.Groups
                .Include(g => g.UserGroups)
                    .ThenInclude(ug => ug.User)
                .Include(g => g.Expenses)
                    .ThenInclude(e => e.UserExpenses)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);
        }

        public async Task<IEnumerable<Group>> GetGroupsByUserIdAsync(int userId)
        {
            return await _context.Groups
                .Include(g => g.UserGroups)
                .Where(g => g.UserGroups.Any(ug => ug.UserId == userId))
                .ToListAsync();
        }

        // Group member operations
        public async Task<bool> AddUserToGroupAsync(int groupId, int userId)
        {
            var existingUserGroup = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.GroupId == groupId && ug.UserId == userId);

            if (existingUserGroup != null) return false; // Already exists

            var userGroup = new UserGroup
            {
                GroupId = groupId,
                UserId = userId
            };

            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveUserFromGroupAsync(int groupId, int userId)
        {
            var userGroup = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.GroupId == groupId && ug.UserId == userId);

            if (userGroup == null) return false;

            _context.UserGroups.Remove(userGroup);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsUserInGroupAsync(int groupId, int userId)
        {
            return await _context.UserGroups
                .AnyAsync(ug => ug.GroupId == groupId && ug.UserId == userId);
        }

        public async Task<IEnumerable<User>> GetGroupMembersAsync(int groupId)
        {
            return await _context.UserGroups
                .Where(ug => ug.GroupId == groupId)
                .Select(ug => ug.User)
                .ToListAsync();
        }

        public async Task<int> GetGroupMemberCountAsync(int groupId)
        {
            return await _context.UserGroups
                .CountAsync(ug => ug.GroupId == groupId);
        }

        // Group expense operations
        public async Task<decimal> GetGroupTotalExpensesAsync(int groupId)
        {
            return await _context.Expenses
                .Where(e => e.GroupId == groupId)
                .SumAsync(e => e.Amount);
        }

        public async Task<IEnumerable<Expense>> GetGroupExpensesAsync(int groupId)
        {
            return await _context.Expenses
                .Include(e => e.UserExpenses)
                    .ThenInclude(ue => ue.User)
                .Where(e => e.GroupId == groupId)
                .OrderByDescending(e => e.ExpenseId)
                .ToListAsync();
        }

        public async Task<int> GetGroupExpenseCountAsync(int groupId)
        {
            return await _context.Expenses
                .CountAsync(e => e.GroupId == groupId);
        }

        // Group balance operations
        public async Task<Dictionary<int, decimal>> GetGroupMemberBalancesAsync(int groupId)
        {
            var balances = new Dictionary<int, decimal>();

            var groupMembers = await GetGroupMembersAsync(groupId);

            foreach (var member in groupMembers)
            {
                var totalPaid = await _context.UserExpenses
                    .Include(ue => ue.Expense)
                    .Where(ue => ue.UserId == member.UserId &&
                                ue.Expense.GroupId == groupId &&
                                ue.Type == UserExpenseType.PAID_BY)
                    .SumAsync(ue => ue.Amount);

                var totalOwed = await _context.UserExpenses
                    .Include(ue => ue.Expense)
                    .Where(ue => ue.UserId == member.UserId &&
                                ue.Expense.GroupId == groupId &&
                                ue.Type == UserExpenseType.HEAD_TO_PAY)
                    .SumAsync(ue => ue.Amount);

                balances[member.UserId] = totalPaid - totalOwed;
            }

            return balances;
        }

        public async Task<bool> HasActiveDebtsAsync(int groupId)
        {
            var balances = await GetGroupMemberBalancesAsync(groupId);
            return balances.Values.Any(balance => Math.Abs(balance) > 0.01m);
        }

        // Search operations
        public async Task<IEnumerable<Group>> SearchGroupsByNameAsync(string name)
        {
            return await _context.Groups
                .Where(g => g.Name.Contains(name))
                .ToListAsync();
        }

        public async Task<IEnumerable<Group>> GetUserActiveGroupsAsync(int userId)
        {
            return await _context.Groups
                .Include(g => g.UserGroups)
                .Include(g => g.Expenses)
                .Where(g => g.UserGroups.Any(ug => ug.UserId == userId) && g.Expenses.Any())
                .ToListAsync();
        }
    }
}