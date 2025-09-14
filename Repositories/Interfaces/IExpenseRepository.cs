using SplitwiseAPI.Models;

namespace SplitwiseAPI.Repositories.Interfaces
{
    public interface IExpenseRepository
    {
        Task<Expense?> GetByIdAsync(int id);
        Task<IEnumerable<Expense>> GetAllAsync();
        Task<Expense> CreateAsync(Expense expense);
        Task<Expense> UpdateAsync(Expense expense);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        Task<Expense?> GetExpenseWithDetailsAsync(int expenseId);
        Task<Expense?> GetExpenseWithGroupAsync(int expenseId);
        Task<Expense?> GetExpenseWithFullDetailsAsync(int expenseId);

        Task<IEnumerable<Expense>> GetExpensesByGroupIdAsync(int groupId);
        Task<IEnumerable<Expense>> GetExpensesByUserIdAsync(int userId);
        Task<IEnumerable<Expense>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate);

        Task<bool> ValidateExpensePasswordAsync(int expenseId, string password);
        Task<bool> IsUserAuthorizedForExpenseAsync(int expenseId, int userId);
        Task<bool> CanUserModifyExpenseAsync(int expenseId, int userId);

        Task<decimal> GetExpenseTotalPaidAsync(int expenseId);
        Task<decimal> GetExpenseTotalToPayAsync(int expenseId);
        Task<Dictionary<int, decimal>> GetExpenseUserBalancesAsync(int expenseId);
        Task<IEnumerable<(int DebtorId, int CreditorId, decimal Amount)>> GetExpenseDebtsAsync(int expenseId);

        Task<decimal> GetTotalExpensesForUserAsync(int userId);
        Task<decimal> GetTotalExpensesForGroupAsync(int groupId);
        Task<IEnumerable<Expense>> GetRecentExpensesAsync(int count = 10);
        Task<IEnumerable<Expense>> GetUserRecentExpensesAsync(int userId, int count = 10);

        Task<IEnumerable<Expense>> SearchExpensesByDescriptionAsync(string description);
        Task<IEnumerable<Expense>> GetExpensesByAmountRangeAsync(decimal minAmount, decimal maxAmount);
    }
}