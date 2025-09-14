using SplitwiseAPI.Models;

namespace SplitwiseAPI.Repositories.Interfaces
{
    public interface IGroupRepository
    {
        Task<Group?> GetByIdAsync(int id);
        Task<IEnumerable<Group>> GetAllAsync();
        Task<Group> CreateAsync(Group group);
        Task<Group> UpdateAsync(Group group);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        Task<Group?> GetGroupWithMembersAsync(int groupId);
        Task<Group?> GetGroupWithExpensesAsync(int groupId);
        Task<Group?> GetGroupWithFullDetailsAsync(int groupId);
        Task<IEnumerable<Group>> GetGroupsByUserIdAsync(int userId);

        Task<bool> AddUserToGroupAsync(int groupId, int userId);
        Task<bool> RemoveUserFromGroupAsync(int groupId, int userId);
        Task<bool> IsUserInGroupAsync(int groupId, int userId);
        Task<IEnumerable<User>> GetGroupMembersAsync(int groupId);
        Task<int> GetGroupMemberCountAsync(int groupId);

        Task<decimal> GetGroupTotalExpensesAsync(int groupId);
        Task<IEnumerable<Expense>> GetGroupExpensesAsync(int groupId);
        Task<int> GetGroupExpenseCountAsync(int groupId);

        Task<Dictionary<int, decimal>> GetGroupMemberBalancesAsync(int groupId);
        Task<bool> HasActiveDebtsAsync(int groupId);

        Task<IEnumerable<Group>> SearchGroupsByNameAsync(string name);
        Task<IEnumerable<Group>> GetUserActiveGroupsAsync(int userId);
    }
}