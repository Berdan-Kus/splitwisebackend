using Microsoft.EntityFrameworkCore;
using SplitwiseAPI.Data;
using SplitwiseAPI.Models;
using SplitwiseAPI.Repositories.Interfaces;

namespace SplitwiseAPI.Repositories.Repositories
{
    public class UserExpenseRepository : IUserExpenseRepository
    {
        private readonly AppDbContext _context;

        public UserExpenseRepository(AppDbContext context)
        {
            _context = context;
        }

        // Basic CRUD operations
        public async Task<UserExpense?> GetByIdAsync(int id)
        {
            return await _context.UserExpenses.FirstOrDefaultAsync(ue => ue.UserExpenseId == id);
        }

        public async Task<IEnumerable<UserExpense>> GetAllAsync()
        {
            return await _context.UserExpenses
                .Include(ue => ue.User)
                .Include(ue => ue.Expense)
                    .ThenInclude(e => e.Group)
                .ToListAsync();
        }

        public async Task<UserExpense> CreateAsync(UserExpense userExpense)
        {
            _context.UserExpenses.Add(userExpense);
            await _context.SaveChangesAsync();
            return userExpense;
        }

        public async Task<UserExpense> UpdateAsync(UserExpense userExpense)
        {
            _context.UserExpenses.Update(userExpense);
            await _context.SaveChangesAsync();
            return userExpense;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var userExpense = await GetByIdAsync(id);
            if (userExpense == null) return false;

            _context.UserExpenses.Remove(userExpense);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.UserExpenses.AnyAsync(ue => ue.UserExpenseId == id);
        }

        // UserExpense-specific operations
        public async Task<UserExpense?> GetUserExpenseWithDetailsAsync(int id)
        {
            return await _context.UserExpenses
                .Include(ue => ue.User)
                .Include(ue => ue.Expense)
                    .ThenInclude(e => e.Group)
                .FirstOrDefaultAsync(ue => ue.UserExpenseId == id);
        }

        public async Task<IEnumerable<UserExpense>> GetUserExpensesByExpenseIdAsync(int expenseId)
        {
            return await _context.UserExpenses
                .Include(ue => ue.User)
                .Where(ue => ue.ExpenseId == expenseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserExpense>> GetUserExpensesByUserIdAsync(int userId)
        {
            return await _context.UserExpenses
                .Include(ue => ue.Expense)
                    .ThenInclude(e => e.Group)
                .Where(ue => ue.UserId == userId)
                .OrderByDescending(ue => ue.UserExpenseId)
                .ToListAsync();
        }

        public async Task<UserExpense?> GetUserExpenseByUserAndExpenseAsync(int userId, int expenseId, UserExpenseType type)
        {
            return await _context.UserExpenses
                .Include(ue => ue.User)
                .Include(ue => ue.Expense)
                .FirstOrDefaultAsync(ue => ue.UserId == userId && ue.ExpenseId == expenseId && ue.Type == type);
        }

        // Bulk operations
        public async Task<IEnumerable<UserExpense>> CreateBulkAsync(IEnumerable<UserExpense> userExpenses)
        {
            _context.UserExpenses.AddRange(userExpenses);
            await _context.SaveChangesAsync();
            return userExpenses;
        }

        public async Task<bool> DeleteByExpenseIdAsync(int expenseId)
        {
            var userExpenses = await _context.UserExpenses
                .Where(ue => ue.ExpenseId == expenseId)
                .ToListAsync();

            if (!userExpenses.Any()) return false;

            _context.UserExpenses.RemoveRange(userExpenses);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByUserIdAsync(int userId)
        {
            var userExpenses = await _context.UserExpenses
                .Where(ue => ue.UserId == userId)
                .ToListAsync();

            if (!userExpenses.Any()) return false;

            _context.UserExpenses.RemoveRange(userExpenses);
            await _context.SaveChangesAsync();
            return true;
        }

        // Balance calculations
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

        public async Task<decimal> GetUserBalanceInGroupAsync(int userId, int groupId)
        {
            var totalPaid = await _context.UserExpenses
                .Include(ue => ue.Expense)
                .Where(ue => ue.UserId == userId && ue.Expense.GroupId == groupId && ue.Type == UserExpenseType.PAID_BY)
                .SumAsync(ue => ue.Amount);

            var totalOwed = await _context.UserExpenses
                .Include(ue => ue.Expense)
                .Where(ue => ue.UserId == userId && ue.Expense.GroupId == groupId && ue.Type == UserExpenseType.HEAD_TO_PAY)
                .SumAsync(ue => ue.Amount);

            return totalPaid - totalOwed;
        }

        // Debt calculations
        public async Task<IEnumerable<(int UserId, decimal Amount)>> GetUserCreditsAsync(int userId)
        {
            // Bu kullanıcının alacaklı olduğu diğer kullanıcılar
            var expenseIds = await _context.UserExpenses
                .Where(ue => ue.UserId == userId && ue.Type == UserExpenseType.PAID_BY)
                .Select(ue => ue.ExpenseId)
                .ToListAsync();

            var debtors = await _context.UserExpenses
                .Where(ue => expenseIds.Contains(ue.ExpenseId) &&
                            ue.UserId != userId &&
                            ue.Type == UserExpenseType.HEAD_TO_PAY)
                .GroupBy(ue => ue.UserId)
                .Select(g => new { UserId = g.Key, Amount = g.Sum(ue => ue.Amount) })
                .ToListAsync();

            return debtors.Select(d => (d.UserId, d.Amount));
        }

        public async Task<IEnumerable<(int UserId, decimal Amount)>> GetUserDebtsAsync(int userId)
        {
            // Bu kullanıcının borçlu olduğu diğer kullanıcılar
            var expenseIds = await _context.UserExpenses
                .Where(ue => ue.UserId == userId && ue.Type == UserExpenseType.HEAD_TO_PAY)
                .Select(ue => ue.ExpenseId)
                .ToListAsync();

            var creditors = await _context.UserExpenses
                .Where(ue => expenseIds.Contains(ue.ExpenseId) &&
                            ue.UserId != userId &&
                            ue.Type == UserExpenseType.PAID_BY)
                .GroupBy(ue => ue.UserId)
                .Select(g => new { UserId = g.Key, Amount = g.Sum(ue => ue.Amount) })
                .ToListAsync();

            return creditors.Select(c => (c.UserId, c.Amount));
        }

        public async Task<Dictionary<int, decimal>> GetUserBalanceWithOthersAsync(int userId)
        {
            var balances = new Dictionary<int, decimal>();

            // Kullanıcının katıldığı tüm masrafları al
            var userExpenses = await _context.UserExpenses
                .Include(ue => ue.Expense)
                    .ThenInclude(e => e.UserExpenses)
                        .ThenInclude(ue => ue.User)
                .Where(ue => ue.UserId == userId)
                .ToListAsync();

            foreach (var userExpense in userExpenses)
            {
                var expense = userExpense.Expense;
                var otherUserExpenses = expense.UserExpenses.Where(ue => ue.UserId != userId);

                foreach (var otherUserExpense in otherUserExpenses)
                {
                    if (!balances.ContainsKey(otherUserExpense.UserId))
                    {
                        balances[otherUserExpense.UserId] = 0;
                    }

                    // Eğer bu kullanıcı ödediyse, diğer kullanıcı borçlu
                    if (userExpense.Type == UserExpenseType.PAID_BY && otherUserExpense.Type == UserExpenseType.HEAD_TO_PAY)
                    {
                        balances[otherUserExpense.UserId] += otherUserExpense.Amount;
                    }
                    // Eğer diğer kullanıcı ödediyse, bu kullanıcı borçlu
                    else if (userExpense.Type == UserExpenseType.HEAD_TO_PAY && otherUserExpense.Type == UserExpenseType.PAID_BY)
                    {
                        balances[otherUserExpense.UserId] -= userExpense.Amount;
                    }
                }
            }

            return balances;
        }

        // Group-based calculations
        public async Task<Dictionary<int, decimal>> GetGroupMemberBalancesAsync(int groupId)
        {
            var groupUsers = await _context.Users
                .Include(u => u.UserGroups)
                .Where(u => u.UserGroups.Any(ug => ug.GroupId == groupId))
                .ToListAsync();

            var balances = new Dictionary<int, decimal>();

            foreach (var user in groupUsers)
            {
                var balance = await GetUserBalanceInGroupAsync(user.UserId, groupId);
                balances[user.UserId] = balance;
            }

            return balances;
        }

        public async Task<IEnumerable<(int DebtorId, int CreditorId, decimal Amount)>> GetGroupDebtsAsync(int groupId)
        {
            var balances = await GetGroupMemberBalancesAsync(groupId);
            var debts = new List<(int DebtorId, int CreditorId, decimal Amount)>();

            var creditors = balances.Where(b => b.Value > 0.01m).ToList();
            var debtors = balances.Where(b => b.Value < -0.01m).ToList();

            foreach (var debtor in debtors)
            {
                var remainingDebt = Math.Abs(debtor.Value);

                foreach (var creditor in creditors)
                {
                    if (remainingDebt <= 0.01m || creditor.Value <= 0.01m) continue;

                    var paymentAmount = Math.Min(remainingDebt, creditor.Value);
                    debts.Add((debtor.Key, creditor.Key, Math.Round(paymentAmount, 2)));

                    remainingDebt -= paymentAmount;
                    // Update creditor balance
                    var creditorIndex = creditors.FindIndex(c => c.Key == creditor.Key);
                    creditors[creditorIndex] = new KeyValuePair<int, decimal>(creditor.Key, creditor.Value - paymentAmount);
                }
            }

            return debts;
        }

        public async Task<IEnumerable<(int DebtorId, int CreditorId, decimal Amount)>> GetSimplifiedGroupDebtsAsync(int groupId)
        {
            // Bu method, minimum transfer sayısı ile borçları hesaplar
            return await GetGroupDebtsAsync(groupId);
        }

        // Expense-based operations
        public async Task<IEnumerable<UserExpense>> GetPaidByUserExpensesAsync(int expenseId)
        {
            return await _context.UserExpenses
                .Include(ue => ue.User)
                .Where(ue => ue.ExpenseId == expenseId && ue.Type == UserExpenseType.PAID_BY)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserExpense>> GetHeadToPayUserExpensesAsync(int expenseId)
        {
            return await _context.UserExpenses
                .Include(ue => ue.User)
                .Where(ue => ue.ExpenseId == expenseId && ue.Type == UserExpenseType.HEAD_TO_PAY)
                .ToListAsync();
        }

        public async Task<decimal> GetExpenseTotalPaidAsync(int expenseId)
        {
            return await _context.UserExpenses
                .Where(ue => ue.ExpenseId == expenseId && ue.Type == UserExpenseType.PAID_BY)
                .SumAsync(ue => ue.Amount);
        }

        public async Task<decimal> GetExpenseTotalOwedAsync(int expenseId)
        {
            return await _context.UserExpenses
                .Where(ue => ue.ExpenseId == expenseId && ue.Type == UserExpenseType.HEAD_TO_PAY)
                .SumAsync(ue => ue.Amount);
        }

        // Statistics and reporting
        public async Task<IEnumerable<UserExpense>> GetUserExpensesByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.UserExpenses
                .Include(ue => ue.Expense)
                    .ThenInclude(e => e.Group)
                .Where(ue => ue.UserId == userId)
                // If you add CreatedDate to UserExpense, use it here
                .OrderByDescending(ue => ue.UserExpenseId)
                .ToListAsync();
        }

        public async Task<decimal> GetUserTotalPaidInDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.UserExpenses
                .Where(ue => ue.UserId == userId && ue.Type == UserExpenseType.PAID_BY)
                // Add date filtering when CreatedDate is available
                .SumAsync(ue => ue.Amount);
        }

        public async Task<decimal> GetUserTotalOwedInDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.UserExpenses
                .Where(ue => ue.UserId == userId && ue.Type == UserExpenseType.HEAD_TO_PAY)
                // Add date filtering when CreatedDate is available
                .SumAsync(ue => ue.Amount);
        }

        // Validation operations
        public async Task<bool> ValidateUserExpenseBalanceAsync(int expenseId)
        {
            var totalPaid = await GetExpenseTotalPaidAsync(expenseId);
            var totalOwed = await GetExpenseTotalOwedAsync(expenseId);

            // Tolerance for decimal precision
            return Math.Abs(totalPaid - totalOwed) < 0.01m;
        }

        public async Task<bool> HasUserPaidForExpenseAsync(int userId, int expenseId)
        {
            return await _context.UserExpenses
                .AnyAsync(ue => ue.UserId == userId && ue.ExpenseId == expenseId && ue.Type == UserExpenseType.PAID_BY);
        }

        public async Task<bool> IsUserOwedForExpenseAsync(int userId, int expenseId)
        {
            return await _context.UserExpenses
                .AnyAsync(ue => ue.UserId == userId && ue.ExpenseId == expenseId && ue.Type == UserExpenseType.HEAD_TO_PAY);
        }
    }
}