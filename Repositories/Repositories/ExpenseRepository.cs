using Microsoft.EntityFrameworkCore;
using SplitwiseAPI.Data;
using SplitwiseAPI.Models;
using SplitwiseAPI.Repositories.Interfaces;

namespace SplitwiseAPI.Repositories.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly AppDbContext _context;

        public ExpenseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Expense?> GetByIdAsync(int id)
        {
            return await _context.Expenses.FirstOrDefaultAsync(e => e.ExpenseId == id);
        }

        public async Task<IEnumerable<Expense>> GetAllAsync()
        {
            return await _context.Expenses
                .Include(e => e.Group)
                .OrderByDescending(e => e.ExpenseId)
                .ToListAsync();
        }

        public async Task<Expense> CreateAsync(Expense expense)
        {
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            return expense;
        }

        public async Task<Expense> UpdateAsync(Expense expense)
        {
            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();
            return expense;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var expense = await GetByIdAsync(id);
            if (expense == null) return false;

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Expenses.AnyAsync(e => e.ExpenseId == id);
        }

        public async Task<Expense?> GetExpenseWithDetailsAsync(int expenseId)
        {
            return await _context.Expenses
                .Include(e => e.UserExpenses)
                    .ThenInclude(ue => ue.User)
                .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);
        }

        public async Task<Expense?> GetExpenseWithGroupAsync(int expenseId)
        {
            return await _context.Expenses
                .Include(e => e.Group)
                    .ThenInclude(g => g.UserGroups)
                        .ThenInclude(ug => ug.User)
                .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);
        }

        public async Task<Expense?> GetExpenseWithFullDetailsAsync(int expenseId)
        {
            return await _context.Expenses
                .Include(e => e.Group)
                    .ThenInclude(g => g.UserGroups)
                        .ThenInclude(ug => ug.User)
                .Include(e => e.UserExpenses)
                    .ThenInclude(ue => ue.User)
                .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);
        }

        public async Task<IEnumerable<Expense>> GetExpensesByGroupIdAsync(int groupId)
        {
            return await _context.Expenses
                .Include(e => e.UserExpenses)
                    .ThenInclude(ue => ue.User)
                .Where(e => e.GroupId == groupId)
                .OrderByDescending(e => e.ExpenseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetExpensesByUserIdAsync(int userId)
        {
            return await _context.Expenses
                .Include(e => e.Group)
                .Include(e => e.UserExpenses)
                    .ThenInclude(ue => ue.User)
                .Where(e => e.UserExpenses.Any(ue => ue.UserId == userId))
                .OrderByDescending(e => e.ExpenseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Expenses
                .Include(e => e.Group)
                .Include(e => e.UserExpenses)
                    .ThenInclude(ue => ue.User)
                .Where(e => e.ExpenseId >= 0) // Placeholder for date filtering if you add CreatedDate
                .OrderByDescending(e => e.ExpenseId)
                .ToListAsync();
        }

        public async Task<bool> ValidateExpensePasswordAsync(int expenseId, string password)
        {
            var expense = await GetByIdAsync(expenseId);
            return expense != null && expense.Password == password;
        }

        public async Task<bool> IsUserAuthorizedForExpenseAsync(int expenseId, int userId)
        {
            return await _context.Expenses
                .Include(e => e.Group)
                    .ThenInclude(g => g.UserGroups)
                .AnyAsync(e => e.ExpenseId == expenseId &&
                              e.Group.UserGroups.Any(ug => ug.UserId == userId));
        }

        public async Task<bool> CanUserModifyExpenseAsync(int expenseId, int userId)
        {
            return await _context.UserExpenses
                .AnyAsync(ue => ue.ExpenseId == expenseId && ue.UserId == userId);
        }

        public async Task<decimal> GetExpenseTotalPaidAsync(int expenseId)
        {
            return await _context.UserExpenses
                .Where(ue => ue.ExpenseId == expenseId && ue.Type == UserExpenseType.PAID_BY)
                .SumAsync(ue => ue.Amount);
        }

        public async Task<decimal> GetExpenseTotalToPayAsync(int expenseId)
        {
            return await _context.UserExpenses
                .Where(ue => ue.ExpenseId == expenseId && ue.Type == UserExpenseType.HEAD_TO_PAY)
                .SumAsync(ue => ue.Amount);
        }

        public async Task<Dictionary<int, decimal>> GetExpenseUserBalancesAsync(int expenseId)
        {
            var userExpenses = await _context.UserExpenses
                .Include(ue => ue.User)
                .Where(ue => ue.ExpenseId == expenseId)
                .ToListAsync();

            var balances = new Dictionary<int, decimal>();

            foreach (var userExpense in userExpenses)
            {
                if (!balances.ContainsKey(userExpense.UserId))
                {
                    balances[userExpense.UserId] = 0;
                }

                if (userExpense.Type == UserExpenseType.PAID_BY)
                {
                    balances[userExpense.UserId] += userExpense.Amount;
                }
                else if (userExpense.Type == UserExpenseType.HEAD_TO_PAY)
                {
                    balances[userExpense.UserId] -= userExpense.Amount;
                }
            }

            return balances;
        }

        public async Task<IEnumerable<(int DebtorId, int CreditorId, decimal Amount)>> GetExpenseDebtsAsync(int expenseId)
        {
            var balances = await GetExpenseUserBalancesAsync(expenseId);
            var debts = new List<(int DebtorId, int CreditorId, decimal Amount)>();

            var creditors = balances.Where(b => b.Value > 0).ToList();
            var debtors = balances.Where(b => b.Value < 0).ToList();

            foreach (var debtor in debtors)
            {
                var remainingDebt = Math.Abs(debtor.Value);

                foreach (var creditor in creditors)
                {
                    if (remainingDebt <= 0 || creditor.Value <= 0) continue;

                    var paymentAmount = Math.Min(remainingDebt, creditor.Value);
                    debts.Add((debtor.Key, creditor.Key, paymentAmount));

                    remainingDebt -= paymentAmount;
                    var creditorIndex = creditors.FindIndex(c => c.Key == creditor.Key);
                    creditors[creditorIndex] = new KeyValuePair<int, decimal>(creditor.Key, creditor.Value - paymentAmount);
                }
            }

            return debts;
        }

        public async Task<decimal> GetTotalExpensesForUserAsync(int userId)
        {
            return await _context.UserExpenses
                .Where(ue => ue.UserId == userId && ue.Type == UserExpenseType.HEAD_TO_PAY)
                .SumAsync(ue => ue.Amount);
        }

        public async Task<decimal> GetTotalExpensesForGroupAsync(int groupId)
        {
            return await _context.Expenses
                .Where(e => e.GroupId == groupId)
                .SumAsync(e => e.Amount);
        }

        public async Task<IEnumerable<Expense>> GetRecentExpensesAsync(int count = 10)
        {
            return await _context.Expenses
                .Include(e => e.Group)
                .Include(e => e.UserExpenses)
                    .ThenInclude(ue => ue.User)
                .OrderByDescending(e => e.ExpenseId)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetUserRecentExpensesAsync(int userId, int count = 10)
        {
            return await _context.Expenses
                .Include(e => e.Group)
                .Include(e => e.UserExpenses)
                    .ThenInclude(ue => ue.User)
                .Where(e => e.UserExpenses.Any(ue => ue.UserId == userId))
                .OrderByDescending(e => e.ExpenseId)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Expense>> SearchExpensesByDescriptionAsync(string description)
        {
            return await _context.Expenses
                .Include(e => e.Group)
                .Include(e => e.UserExpenses)
                    .ThenInclude(ue => ue.User)
                .Where(e => e.Description.Contains(description))
                .OrderByDescending(e => e.ExpenseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Expense>> GetExpensesByAmountRangeAsync(decimal minAmount, decimal maxAmount)
        {
            return await _context.Expenses
                .Include(e => e.Group)
                .Include(e => e.UserExpenses)
                    .ThenInclude(ue => ue.User)
                .Where(e => e.Amount >= minAmount && e.Amount <= maxAmount)
                .OrderByDescending(e => e.ExpenseId)
                .ToListAsync();
        }
    }
}