using SplitwiseAPI.DTOs.GroupDTOs;
using SplitwiseAPI.DTOs.UserExpenseDTOs;

namespace SplitwiseAPI.Services.Interfaces
{
    public interface IGroupService
    {
        // Basic CRUD operations
        Task<GroupResponseDto?> GetGroupByIdAsync(int id);
        Task<IEnumerable<GroupResponseDto>> GetAllGroupsAsync();
        Task<GroupResponseDto> CreateGroupAsync(CreateGroupDto createGroupDto);
        Task<GroupResponseDto?> UpdateGroupAsync(int id, UpdateGroupDto updateGroupDto);
        Task<bool> DeleteGroupAsync(int id);

        // Group validation
        Task<bool> GroupExistsAsync(int id);
        Task<bool> IsUserInGroupAsync(int groupId, int userId);
        Task<bool> CanUserAccessGroupAsync(int groupId, int userId);

        // Group member management
        Task<bool> AddUserToGroupAsync(int groupId, int userId);
        Task<bool> RemoveUserFromGroupAsync(int groupId, int userId);
        Task<bool> AddUsersToGroupAsync(int groupId, ManageGroupMembersDto manageGroupMembersDto);
        Task<bool> RemoveUsersFromGroupAsync(int groupId, ManageGroupMembersDto manageGroupMembersDto);
        Task<IEnumerable<GroupMemberDto>> GetGroupMembersAsync(int groupId);

        // Group details with navigation properties
        Task<GroupResponseDto?> GetGroupWithMembersAsync(int groupId);
        Task<GroupResponseDto?> GetGroupWithExpensesAsync(int groupId);
        Task<GroupResponseDto?> GetGroupWithFullDetailsAsync(int groupId);

        // User's groups
        Task<IEnumerable<GroupResponseDto>> GetGroupsByUserIdAsync(int userId);
        Task<IEnumerable<GroupResponseDto>> GetUserActiveGroupsAsync(int userId);

        // Group financial operations
        Task<GroupBalanceSummaryDto> GetGroupBalanceSummaryAsync(int groupId);
        Task<IEnumerable<SimplifiedDebtDto>> GetGroupSimplifiedDebtsAsync(int groupId);
        Task<decimal> GetGroupTotalExpensesAsync(int groupId);
        Task<bool> HasGroupActiveDebtsAsync(int groupId);

        // Group statistics
        Task<int> GetGroupMemberCountAsync(int groupId);
        Task<int> GetGroupExpenseCountAsync(int groupId);
        Task<Dictionary<int, decimal>> GetGroupMemberBalancesAsync(int groupId);

        // Search operations
        Task<IEnumerable<GroupResponseDto>> SearchGroupsByNameAsync(string name);

        // Group validation for operations
        Task<bool> ValidateGroupMembershipAsync(int groupId, IEnumerable<int> userIds);
        Task<bool> CanDeleteGroupAsync(int groupId);
    }
}