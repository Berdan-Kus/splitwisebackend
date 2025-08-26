using SplitwiseAPI.Models;

namespace SplitwiseAPI.Repositories.Interfaces
{
    public interface IUserExpenseRepository
    {
        // Basic CRUD operations
        Task<UserExpense?> GetByIdAsync(int id);
        Task<IEnumerable<UserExpense>> GetAllAsync();
        Task<UserExpense> CreateAsync(UserExpense userExpense);
        Task<UserExpense> UpdateAsync(UserExpense userExpense);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // UserExpense-specific operations
        Task<UserExpense?> GetUserExpenseWithDetailsAsync(int id);
        Task<IEnumerable<UserExpense>> GetUserExpensesByExpenseIdAsync(int expenseId);
        Task<IEnumerable<UserExpense>> GetUserExpensesByUserIdAsync(int userId);
        Task<UserExpense?> GetUserExpenseByUserAndExpenseAsync(int userId, int expenseId, UserExpenseType type);

        // Bulk operations
        Task<IEnumerable<UserExpense>> CreateBulkAsync(IEnumerable<UserExpense> userExpenses);
        Task<bool> DeleteByExpenseIdAsync(int expenseId);
        Task<bool> DeleteByUserIdAsync(int userId);

        // Balance calculations
        Task<decimal> GetUserTotalPaidAsync(int userId);
        Task<decimal> GetUserTotalOwedAsync(int userId);
        Task<decimal> GetUserNetBalanceAsync(int userId);
        Task<decimal> GetUserBalanceInGroupAsync(int userId, int groupId);

        // Debt calculations
        Task<IEnumerable<(int UserId, decimal Amount)>> GetUserCreditsAsync(int userId);
        Task<IEnumerable<(int UserId, decimal Amount)>> GetUserDebtsAsync(int userId);
        Task<Dictionary<int, decimal>> GetUserBalanceWithOthersAsync(int userId);

        // Group-based calculations
        Task<Dictionary<int, decimal>> GetGroupMemberBalancesAsync(int groupId);
        Task<IEnumerable<(int DebtorId, int CreditorId, decimal Amount)>> GetGroupDebtsAsync(int groupId);
        Task<IEnumerable<(int DebtorId, int CreditorId, decimal Amount)>> GetSimplifiedGroupDebtsAsync(int groupId);

        // Expense-based operations
        Task<IEnumerable<UserExpense>> GetPaidByUserExpensesAsync(int expenseId);
        Task<IEnumerable<UserExpense>> GetHeadToPayUserExpensesAsync(int expenseId);
        Task<decimal> GetExpenseTotalPaidAsync(int expenseId);
        Task<decimal> GetExpenseTotalOwedAsync(int expenseId);

        // Statistics and reporting
        Task<IEnumerable<UserExpense>> GetUserExpensesByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
        Task<decimal> GetUserTotalPaidInDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
        Task<decimal> GetUserTotalOwedInDateRangeAsync(int userId, DateTime startDate, DateTime endDate);

        // Validation operations
        Task<bool> ValidateUserExpenseBalanceAsync(int expenseId);
        Task<bool> HasUserPaidForExpenseAsync(int userId, int expenseId);
        Task<bool> IsUserOwedForExpenseAsync(int userId, int expenseId);
    }
}