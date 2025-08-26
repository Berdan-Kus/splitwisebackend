using SplitwiseAPI.DTOs.ExpenseDTOs;

namespace SplitwiseAPI.Services.Interfaces
{
    public interface IExpenseService
    {
        // Basic CRUD operations
        Task<ExpenseResponseDto?> GetExpenseByIdAsync(int id);
        Task<IEnumerable<ExpenseListDto>> GetAllExpensesAsync();
        Task<ExpenseResponseDto> CreateExpenseAsync(CreateExpenseDto createExpenseDto);
        Task<ExpenseResponseDto> CreateSimpleExpenseAsync(SimpleExpenseDto simpleExpenseDto);
        Task<ExpenseResponseDto?> UpdateExpenseAsync(int id, UpdateExpenseDto updateExpenseDto);
        Task<bool> DeleteExpenseAsync(int id);

        // Expense validation
        Task<bool> ExpenseExistsAsync(int id);
        Task<bool> ValidateExpensePasswordAsync(int expenseId, string password);
        Task<bool> IsUserAuthorizedForExpenseAsync(int expenseId, int userId);
        Task<bool> CanUserModifyExpenseAsync(int expenseId, int userId);

        // Expense details
        Task<ExpenseResponseDto?> GetExpenseWithDetailsAsync(int expenseId);
        Task<ExpenseResponseDto?> GetExpenseWithGroupAsync(int expenseId);
        Task<ExpenseResponseDto?> GetExpenseWithFullDetailsAsync(int expenseId);

        // Group and user expenses
        Task<IEnumerable<ExpenseListDto>> GetExpensesByGroupIdAsync(int groupId);
        Task<IEnumerable<ExpenseListDto>> GetExpensesByUserIdAsync(int userId);
        Task<IEnumerable<ExpenseListDto>> GetUserRecentExpensesAsync(int userId, int count = 10);
        Task<IEnumerable<ExpenseListDto>> GetRecentExpensesAsync(int count = 10);

        // Expense calculations
        Task<ExpenseSummaryDto> GetExpenseSummaryAsync(int expenseId);
        Task<IEnumerable<DebtDto>> GetExpenseDebtsAsync(int expenseId);
        Task<Dictionary<int, decimal>> GetExpenseUserBalancesAsync(int expenseId);
        Task<decimal> GetExpenseTotalPaidAsync(int expenseId);
        Task<decimal> GetExpenseTotalToPayAsync(int expenseId);

        // Expense business logic
        Task<bool> ValidateExpenseBalanceAsync(int expenseId);
        Task<ExpenseResponseDto> RecalculateExpenseAsync(int expenseId);

        // Search and filter operations
        Task<IEnumerable<ExpenseListDto>> SearchExpensesByDescriptionAsync(string description);
        Task<IEnumerable<ExpenseListDto>> GetExpensesByAmountRangeAsync(decimal minAmount, decimal maxAmount);
        Task<IEnumerable<ExpenseListDto>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Statistics
        Task<decimal> GetTotalExpensesForUserAsync(int userId);
        Task<decimal> GetTotalExpensesForGroupAsync(int groupId);

        // Expense management
        Task<bool> SettleExpenseAsync(int expenseId, string password);
        Task<ExpenseResponseDto> SplitExpenseEquallyAsync(int expenseId, IEnumerable<int> participantIds);
    }
}