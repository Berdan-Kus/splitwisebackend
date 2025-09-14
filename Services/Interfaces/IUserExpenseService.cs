using SplitwiseAPI.DTOs.UserExpenseDTOs;

namespace SplitwiseAPI.Services.Interfaces
{
    public interface IUserExpenseService
    {
        Task<UserExpenseResponseDto?> GetUserExpenseByIdAsync(int id);
        Task<IEnumerable<UserExpenseResponseDto>> GetAllUserExpensesAsync();
        Task<UserExpenseResponseDto> CreateUserExpenseAsync(CreateUserExpenseDto createUserExpenseDto);
        Task<IEnumerable<UserExpenseResponseDto>> CreateBulkUserExpenseAsync(CreateBulkUserExpenseDto createBulkUserExpenseDto);
        Task<UserExpenseResponseDto?> UpdateUserExpenseAsync(int id, UpdateUserExpenseDto updateUserExpenseDto);
        Task<bool> DeleteUserExpenseAsync(int id);

        Task<IEnumerable<UserExpenseResponseDto>> GetUserExpensesByExpenseIdAsync(int expenseId);
        Task<IEnumerable<UserExpenseResponseDto>> GetUserExpensesByUserIdAsync(int userId);
        Task<UserExpenseResponseDto?> GetUserExpenseByUserAndExpenseAsync(int userId, int expenseId, string type);

        Task<UserBalanceDto> GetUserBalanceAsync(int userId);
        Task<UserBalanceDto> GetUserBalanceInGroupAsync(int userId, int groupId);
        Task<GroupBalanceSummaryDto> GetGroupBalanceSummaryAsync(int groupId);

        Task<IEnumerable<UserDebtDetailDto>> GetUserDebtDetailsAsync(int userId);
        Task<IEnumerable<SimplifiedDebtDto>> GetSimplifiedGroupDebtsAsync(int groupId);
        Task<Dictionary<int, decimal>> GetUserBalanceWithOthersAsync(int userId);

        Task<bool> SettleDebtAsync(SettleDebtDto settleDebtDto);
        Task<bool> SettlePartialDebtAsync(int debtorUserId, int creditorUserId, decimal amount, string? note = null);
        Task<SettlementHistoryDto> RecordSettlementAsync(int debtorUserId, int creditorUserId, decimal amount, string? note = null);

        Task<UserDashboardDto> GetUserDashboardAsync(int userId);
        Task<IEnumerable<RecentActivityDto>> GetUserRecentActivitiesAsync(int userId, int count = 10);
        Task<IEnumerable<ActiveDebtDto>> GetUserActiveDebtsAsync(int userId);

        Task<decimal> GetUserTotalPaidAsync(int userId);
        Task<decimal> GetUserTotalOwedAsync(int userId);
        Task<decimal> GetUserNetBalanceAsync(int userId);
        Task<IEnumerable<UserExpenseResponseDto>> GetUserExpensesByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);

        Task<bool> ValidateUserExpenseBalanceAsync(int expenseId);
        Task<bool> HasUserPaidForExpenseAsync(int userId, int expenseId);
        Task<bool> IsUserOwedForExpenseAsync(int userId, int expenseId);
        Task<bool> UserExpenseExistsAsync(int id);

        Task<bool> DeleteUserExpensesByExpenseIdAsync(int expenseId);
        Task<bool> DeleteUserExpensesByUserIdAsync(int userId);

        Task<IEnumerable<(int DebtorId, int CreditorId, decimal Amount)>> CalculateOptimalSettlementsAsync(int groupId);
        Task<decimal> CalculateMinimumTransfersAsync(int groupId);

        Task<IEnumerable<UserExpenseResponseDto>> GetPaidByUserExpensesAsync(int expenseId);
        Task<IEnumerable<UserExpenseResponseDto>> GetHeadToPayUserExpensesAsync(int expenseId);
    }
}