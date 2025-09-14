using SplitwiseAPI.DTOs.UserExpenseDTOs;
using SplitwiseAPI.Models;
using SplitwiseAPI.Repositories.Interfaces;
using SplitwiseAPI.Services.Interfaces;

namespace SplitwiseAPI.Services.Services
{
    public class UserExpenseService : IUserExpenseService
    {
        private readonly IUserExpenseRepository _userExpenseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IGroupRepository _groupRepository;

        public UserExpenseService(
            IUserExpenseRepository userExpenseRepository,
            IUserRepository userRepository,
            IExpenseRepository expenseRepository,
            IGroupRepository groupRepository)
        {
            _userExpenseRepository = userExpenseRepository;
            _userRepository = userRepository;
            _expenseRepository = expenseRepository;
            _groupRepository = groupRepository;
        }


        public async Task<UserExpenseResponseDto?> GetUserExpenseByIdAsync(int id)
        {
            var userExpense = await _userExpenseRepository.GetUserExpenseWithDetailsAsync(id);
            return userExpense != null ? MapToResponseDto(userExpense) : null;
        }

        public async Task<IEnumerable<UserExpenseResponseDto>> GetAllUserExpensesAsync()
        {
            var userExpenses = await _userExpenseRepository.GetAllAsync();
            return userExpenses.Select(MapToResponseDto);
        }

        public async Task<UserExpenseResponseDto> CreateUserExpenseAsync(CreateUserExpenseDto createUserExpenseDto)
        {

            if (!await _userRepository.ExistsAsync(createUserExpenseDto.UserId))
                throw new ArgumentException("User not found");

            if (!await _expenseRepository.ExistsAsync(createUserExpenseDto.ExpenseId))
                throw new ArgumentException("Expense not found");

            var userExpenseType = createUserExpenseDto.Type.ToUpper() == "PAID_BY"
                ? UserExpenseType.PAID_BY
                : UserExpenseType.HEAD_TO_PAY;

            var userExpense = new UserExpense
            {
                UserId = createUserExpenseDto.UserId,
                ExpenseId = createUserExpenseDto.ExpenseId,
                Amount = createUserExpenseDto.Amount,
                Type = userExpenseType
            };

            var createdUserExpense = await _userExpenseRepository.CreateAsync(userExpense);
            return MapToResponseDto(createdUserExpense);
        }

        public async Task<IEnumerable<UserExpenseResponseDto>> CreateBulkUserExpenseAsync(CreateBulkUserExpenseDto createBulkUserExpenseDto)
        {
            if (!await _expenseRepository.ExistsAsync(createBulkUserExpenseDto.ExpenseId))
                throw new ArgumentException("Expense not found");

            var userExpenses = new List<UserExpense>();
            foreach (var item in createBulkUserExpenseDto.UserExpenses)
            {
                if (!await _userRepository.ExistsAsync(item.UserId))
                    throw new ArgumentException($"User {item.UserId} not found");

                var userExpenseType = item.Type.ToUpper() == "PAID_BY"
                    ? UserExpenseType.PAID_BY
                    : UserExpenseType.HEAD_TO_PAY;

                userExpenses.Add(new UserExpense
                {
                    UserId = item.UserId,
                    ExpenseId = createBulkUserExpenseDto.ExpenseId,
                    Amount = item.Amount,
                    Type = userExpenseType
                });
            }

            var createdUserExpenses = await _userExpenseRepository.CreateBulkAsync(userExpenses);
            return createdUserExpenses.Select(MapToResponseDto);
        }

        public async Task<UserExpenseResponseDto?> UpdateUserExpenseAsync(int id, UpdateUserExpenseDto updateUserExpenseDto)
        {
            var userExpense = await _userExpenseRepository.GetByIdAsync(id);
            if (userExpense == null) return null;

            if (updateUserExpenseDto.Amount.HasValue)
                userExpense.Amount = updateUserExpenseDto.Amount.Value;

            if (!string.IsNullOrEmpty(updateUserExpenseDto.Type))
            {
                userExpense.Type = updateUserExpenseDto.Type.ToUpper() == "PAID_BY"
                    ? UserExpenseType.PAID_BY
                    : UserExpenseType.HEAD_TO_PAY;
            }

            var updatedUserExpense = await _userExpenseRepository.UpdateAsync(userExpense);
            return MapToResponseDto(updatedUserExpense);
        }

        public async Task<bool> DeleteUserExpenseAsync(int id)
        {
            return await _userExpenseRepository.DeleteAsync(id);
        }


        public async Task<IEnumerable<UserExpenseResponseDto>> GetUserExpensesByExpenseIdAsync(int expenseId)
        {
            var userExpenses = await _userExpenseRepository.GetUserExpensesByExpenseIdAsync(expenseId);
            return userExpenses.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<UserExpenseResponseDto>> GetUserExpensesByUserIdAsync(int userId)
        {
            var userExpenses = await _userExpenseRepository.GetUserExpensesByUserIdAsync(userId);
            return userExpenses.Select(MapToResponseDto);
        }

        public async Task<UserExpenseResponseDto?> GetUserExpenseByUserAndExpenseAsync(int userId, int expenseId, string type)
        {
            var userExpenseType = type.ToUpper() == "PAID_BY"
                ? UserExpenseType.PAID_BY
                : UserExpenseType.HEAD_TO_PAY;

            var userExpense = await _userExpenseRepository.GetUserExpenseByUserAndExpenseAsync(userId, expenseId, userExpenseType);
            return userExpense != null ? MapToResponseDto(userExpense) : null;
        }


        public async Task<UserBalanceDto> GetUserBalanceAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new ArgumentException("User not found");

            var totalPaid = await _userExpenseRepository.GetUserTotalPaidAsync(userId);
            var totalOwed = await _userExpenseRepository.GetUserTotalOwedAsync(userId);
            var netBalance = totalPaid - totalOwed;

            var balanceWithOthers = await _userExpenseRepository.GetUserBalanceWithOthersAsync(userId);
            var debtDetails = await BuildDebtDetailsAsync(userId, balanceWithOthers);

            return new UserBalanceDto
            {
                UserId = userId,
                UserName = user.Name,
                TotalPaid = totalPaid,
                TotalOwed = totalOwed,
                NetBalance = netBalance,
                DebtDetails = debtDetails
            };
        }

        public async Task<UserBalanceDto> GetUserBalanceInGroupAsync(int userId, int groupId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new ArgumentException("User not found");

            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null) throw new ArgumentException("Group not found");

            var netBalance = await _userExpenseRepository.GetUserBalanceInGroupAsync(userId, groupId);

            return new UserBalanceDto
            {
                UserId = userId,
                UserName = user.Name,
                TotalPaid = 0, // Will need separate calculation for group-specific totals
                TotalOwed = 0,
                NetBalance = netBalance,
                DebtDetails = new List<UserDebtDetailDto>() // Can be populated with group-specific debts
            };
        }

        public async Task<GroupBalanceSummaryDto> GetGroupBalanceSummaryAsync(int groupId)
        {
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null) throw new ArgumentException("Group not found");

            var totalExpenses = await _groupRepository.GetGroupTotalExpensesAsync(groupId);
            var memberBalances = await _userExpenseRepository.GetGroupMemberBalancesAsync(groupId);
            var simplifiedDebts = await GetSimplifiedGroupDebtsAsync(groupId);

            var userBalanceSummaries = new List<UserBalanceSummaryDto>();
            foreach (var balance in memberBalances)
            {
                var user = await _userRepository.GetByIdAsync(balance.Key);
                if (user != null)
                {
                    var totalPaid = await _userExpenseRepository.GetUserTotalPaidAsync(balance.Key);
                    var totalOwed = await _userExpenseRepository.GetUserTotalOwedAsync(balance.Key);

                    userBalanceSummaries.Add(new UserBalanceSummaryDto
                    {
                        UserId = balance.Key,
                        UserName = user.Name,
                        TotalPaid = totalPaid,
                        TotalOwed = totalOwed,
                        NetBalance = balance.Value
                    });
                }
            }

            return new GroupBalanceSummaryDto
            {
                GroupId = groupId,
                GroupName = group.Name,
                TotalExpenses = totalExpenses,
                UserBalances = userBalanceSummaries,
                SimplifiedDebts = simplifiedDebts.ToList()
            };
        }


        public async Task<IEnumerable<UserDebtDetailDto>> GetUserDebtDetailsAsync(int userId)
        {
            try
            {
                var balanceWithOthers = await _userExpenseRepository.GetUserBalanceWithOthersAsync(userId);
                var debtDetails = new List<UserDebtDetailDto>();

                foreach (var balance in balanceWithOthers.Where(b => Math.Abs(b.Value) > 0.01m))
                {
                    var otherUser = await _userRepository.GetByIdAsync(balance.Key);
                    if (otherUser != null)
                    {
                        debtDetails.Add(new UserDebtDetailDto
                        {
                            OtherUserId = balance.Key,
                            OtherUserName = otherUser.Name,
                            Amount = Math.Abs(balance.Value),
                            Type = balance.Value > 0 ? "OWED" : "OWE",
                            RelatedExpenses = new List<DebtExpenseDto>() // Simplified for now
                        });
                    }
                }

                return debtDetails;
            }
            catch (Exception ex)
            {

                return new List<UserDebtDetailDto>();
            }
        }

        public async Task<IEnumerable<SimplifiedDebtDto>> GetSimplifiedGroupDebtsAsync(int groupId)
        {
            try
            {
                var debts = await _userExpenseRepository.GetSimplifiedGroupDebtsAsync(groupId);
                var simplifiedDebts = new List<SimplifiedDebtDto>();

                foreach (var debt in debts)
                {
                    var fromUser = await _userRepository.GetByIdAsync(debt.DebtorId);
                    var toUser = await _userRepository.GetByIdAsync(debt.CreditorId);

                    if (fromUser != null && toUser != null)
                    {
                        simplifiedDebts.Add(new SimplifiedDebtDto
                        {
                            FromUserId = debt.DebtorId,
                            FromUserName = fromUser.Name,
                            ToUserId = debt.CreditorId,
                            ToUserName = toUser.Name,
                            Amount = debt.Amount
                        });
                    }
                }

                return simplifiedDebts;
            }
            catch (Exception ex)
            {

                return new List<SimplifiedDebtDto>();
            }
        }

        public async Task<Dictionary<int, decimal>> GetUserBalanceWithOthersAsync(int userId)
        {
            return await _userExpenseRepository.GetUserBalanceWithOthersAsync(userId);
        }


        public async Task<bool> SettleDebtAsync(SettleDebtDto settleDebtDto)
        {
            return await SettlePartialDebtAsync(
                settleDebtDto.DebtorUserId,
                settleDebtDto.CreditorUserId,
                settleDebtDto.Amount,
                settleDebtDto.Note);
        }

        public async Task<bool> SettlePartialDebtAsync(int debtorUserId, int creditorUserId, decimal amount, string? note = null)
        {

            if (!await _userRepository.ExistsAsync(debtorUserId))
                throw new ArgumentException("Debtor user not found");

            if (!await _userRepository.ExistsAsync(creditorUserId))
                throw new ArgumentException("Creditor user not found");


            var balances = await _userExpenseRepository.GetUserBalanceWithOthersAsync(debtorUserId);
            if (!balances.ContainsKey(creditorUserId) || balances[creditorUserId] >= 0)
                throw new InvalidOperationException("No debt found between these users");

            var currentDebt = Math.Abs(balances[creditorUserId]);
            if (amount > currentDebt)
                throw new InvalidOperationException("Settlement amount exceeds current debt");


            var settlementExpense = new Expense
            {
                Description = $"Settlement: {note ?? "Debt payment"}",
                Amount = amount,
                Password = "settlement", // Special password for settlement expenses
                GroupId = 1 // Use first group or create a special "Settlement" group
            };

            var createdExpense = await _expenseRepository.CreateAsync(settlementExpense);


            var settlementUserExpenses = new List<UserExpense>
            {

                new UserExpense
                {
                    UserId = debtorUserId,
                    ExpenseId = createdExpense.ExpenseId,
                    Amount = amount,
                    Type = UserExpenseType.PAID_BY
                },

                new UserExpense
                {
                    UserId = creditorUserId,
                    ExpenseId = createdExpense.ExpenseId,
                    Amount = amount,
                    Type = UserExpenseType.HEAD_TO_PAY
                }
            };

            await _userExpenseRepository.CreateBulkAsync(settlementUserExpenses);

            return true;
        }

        public async Task<SettlementHistoryDto> RecordSettlementAsync(int debtorUserId, int creditorUserId, decimal amount, string? note = null)
        {


            var debtorUser = await _userRepository.GetByIdAsync(debtorUserId);
            var creditorUser = await _userRepository.GetByIdAsync(creditorUserId);

            return new SettlementHistoryDto
            {
                SettlementId = new Random().Next(1, 1000000), // In real app, this would be from database
                DebtorUserId = debtorUserId,
                DebtorUserName = debtorUser?.Name ?? "Unknown",
                CreditorUserId = creditorUserId,
                CreditorUserName = creditorUser?.Name ?? "Unknown",
                Amount = amount,
                SettlementDate = DateTime.Now,
                Note = note,
                GroupName = "Multiple Groups" // You might need to specify which group this settlement affects
            };
        }


        public async Task<UserDashboardDto> GetUserDashboardAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new ArgumentException("User not found");

            var netBalance = await _userExpenseRepository.GetUserNetBalanceAsync(userId);
            var recentActivities = await GetUserRecentActivitiesAsync(userId, 5);
            var activeDebts = await GetUserActiveDebtsAsync(userId);

            return new UserDashboardDto
            {
                UserId = userId,
                UserName = user.Name,
                TotalNetBalance = netBalance,
                ActiveGroupsCount = 0, // Will be calculated from user groups
                TotalExpensesCount = 0, // Will be calculated from user expenses
                RecentActivities = recentActivities.ToList(),
                ActiveDebts = activeDebts.ToList()
            };
        }

        public async Task<IEnumerable<RecentActivityDto>> GetUserRecentActivitiesAsync(int userId, int count = 10)
        {
            var userExpenses = await _userExpenseRepository.GetUserExpensesByUserIdAsync(userId);
            var recentActivities = new List<RecentActivityDto>();

            foreach (var ue in userExpenses.Take(count))
            {
                var expense = await _expenseRepository.GetExpenseWithGroupAsync(ue.ExpenseId);
                if (expense != null)
                {
                    recentActivities.Add(new RecentActivityDto
                    {
                        ExpenseId = ue.ExpenseId,
                        Description = expense.Description,
                        Amount = ue.Amount,
                        GroupName = expense.Group?.Name ?? "Unknown",
                        ActivityType = ue.Type == UserExpenseType.PAID_BY ? "PAID" : "SHARED",
                        Date = DateTime.Now // You might want to add a CreatedDate field
                    });
                }
            }

            return recentActivities;
        }

        public async Task<IEnumerable<ActiveDebtDto>> GetUserActiveDebtsAsync(int userId)
        {
            var balances = await _userExpenseRepository.GetUserBalanceWithOthersAsync(userId);
            var activeDebts = new List<ActiveDebtDto>();

            foreach (var balance in balances.Where(b => Math.Abs(b.Value) > 0.01m))
            {
                var otherUser = await _userRepository.GetByIdAsync(balance.Key);
                if (otherUser != null)
                {
                    activeDebts.Add(new ActiveDebtDto
                    {
                        OtherUserId = balance.Key,
                        OtherUserName = otherUser.Name,
                        Amount = Math.Abs(balance.Value),
                        Type = balance.Value > 0 ? "OWED" : "OWE",
                        ExpenseCount = 1 // Could be calculated more precisely
                    });
                }
            }

            return activeDebts;
        }


        public async Task<decimal> GetUserTotalPaidAsync(int userId)
        {
            return await _userExpenseRepository.GetUserTotalPaidAsync(userId);
        }

        public async Task<decimal> GetUserTotalOwedAsync(int userId)
        {
            return await _userExpenseRepository.GetUserTotalOwedAsync(userId);
        }

        public async Task<decimal> GetUserNetBalanceAsync(int userId)
        {
            return await _userExpenseRepository.GetUserNetBalanceAsync(userId);
        }

        public async Task<IEnumerable<UserExpenseResponseDto>> GetUserExpensesByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            var userExpenses = await _userExpenseRepository.GetUserExpensesByDateRangeAsync(userId, startDate, endDate);
            return userExpenses.Select(MapToResponseDto);
        }


        public async Task<bool> ValidateUserExpenseBalanceAsync(int expenseId)
        {
            return await _userExpenseRepository.ValidateUserExpenseBalanceAsync(expenseId);
        }

        public async Task<bool> HasUserPaidForExpenseAsync(int userId, int expenseId)
        {
            return await _userExpenseRepository.HasUserPaidForExpenseAsync(userId, expenseId);
        }

        public async Task<bool> IsUserOwedForExpenseAsync(int userId, int expenseId)
        {
            return await _userExpenseRepository.IsUserOwedForExpenseAsync(userId, expenseId);
        }

        public async Task<bool> UserExpenseExistsAsync(int id)
        {
            return await _userExpenseRepository.ExistsAsync(id);
        }


        public async Task<bool> DeleteUserExpensesByExpenseIdAsync(int expenseId)
        {
            return await _userExpenseRepository.DeleteByExpenseIdAsync(expenseId);
        }

        public async Task<bool> DeleteUserExpensesByUserIdAsync(int userId)
        {
            return await _userExpenseRepository.DeleteByUserIdAsync(userId);
        }


        public async Task<IEnumerable<(int DebtorId, int CreditorId, decimal Amount)>> CalculateOptimalSettlementsAsync(int groupId)
        {
            return await _userExpenseRepository.GetSimplifiedGroupDebtsAsync(groupId);
        }

        public async Task<decimal> CalculateMinimumTransfersAsync(int groupId)
        {
            var debts = await _userExpenseRepository.GetSimplifiedGroupDebtsAsync(groupId);
            return debts.Count();
        }


        public async Task<IEnumerable<UserExpenseResponseDto>> GetPaidByUserExpensesAsync(int expenseId)
        {
            var userExpenses = await _userExpenseRepository.GetPaidByUserExpensesAsync(expenseId);
            return userExpenses.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<UserExpenseResponseDto>> GetHeadToPayUserExpensesAsync(int expenseId)
        {
            var userExpenses = await _userExpenseRepository.GetHeadToPayUserExpensesAsync(expenseId);
            return userExpenses.Select(MapToResponseDto);
        }


        private static UserExpenseResponseDto MapToResponseDto(UserExpense userExpense)
        {
            return new UserExpenseResponseDto
            {
                UserExpenseId = userExpense.UserExpenseId,
                Amount = userExpense.Amount,
                Type = userExpense.Type.ToString(),
                User = new UserExpenseUserDto
                {
                    UserId = userExpense.User?.UserId ?? userExpense.UserId,
                    Name = userExpense.User?.Name ?? "Unknown",
                    Phone = userExpense.User?.Phone ?? "Unknown"
                },
                Expense = new UserExpenseExpenseDto
                {
                    ExpenseId = userExpense.Expense?.ExpenseId ?? userExpense.ExpenseId,
                    Description = userExpense.Expense?.Description ?? "Unknown",
                    TotalAmount = userExpense.Expense?.Amount ?? 0,
                    GroupName = userExpense.Expense?.Group?.Name ?? "Unknown"
                }
            };
        }

        private async Task<List<UserDebtDetailDto>> BuildDebtDetailsAsync(int userId, Dictionary<int, decimal> balanceWithOthers)
        {
            var debtDetails = new List<UserDebtDetailDto>();

            foreach (var balance in balanceWithOthers.Where(b => Math.Abs(b.Value) > 0.01m))
            {
                var otherUser = await _userRepository.GetByIdAsync(balance.Key);
                if (otherUser != null)
                {
                    debtDetails.Add(new UserDebtDetailDto
                    {
                        OtherUserId = balance.Key,
                        OtherUserName = otherUser.Name,
                        Amount = Math.Abs(balance.Value),
                        Type = balance.Value > 0 ? "OWED" : "OWE",
                        RelatedExpenses = new List<DebtExpenseDto>() // Can be populated with related expenses
                    });
                }
            }

            return debtDetails;
        }
    }
}