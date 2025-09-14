using SplitwiseAPI.DTOs.ExpenseDTOs;
using SplitwiseAPI.Models;
using SplitwiseAPI.Repositories.Interfaces;
using SplitwiseAPI.Services.Interfaces;

namespace SplitwiseAPI.Services.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IUserExpenseRepository _userExpenseRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IUserRepository _userRepository;

        public ExpenseService(
            IExpenseRepository expenseRepository,
            IUserExpenseRepository userExpenseRepository,
            IGroupRepository groupRepository,
            IUserRepository userRepository)
        {
            _expenseRepository = expenseRepository;
            _userExpenseRepository = userExpenseRepository;
            _groupRepository = groupRepository;
            _userRepository = userRepository;
        }


        public async Task<ExpenseResponseDto?> GetExpenseByIdAsync(int id)
        {
            var expense = await _expenseRepository.GetByIdAsync(id);
            if (expense == null) return null;


            var group = await _groupRepository.GetByIdAsync(expense.GroupId);

            return new ExpenseResponseDto
            {
                ExpenseId = expense.ExpenseId,
                Description = expense.Description,
                Amount = expense.Amount,
                Group = new ExpenseGroupDto
                {
                    GroupId = expense.GroupId,
                    GroupName = group?.Name ?? "Unknown"
                },
                ExpenseDetails = new List<ExpenseDetailDto>(), // Boş liste
                Summary = new ExpenseSummaryDto
                {
                    TotalPaid = expense.Amount,
                    TotalToPay = expense.Amount,
                    Debts = new List<DebtDto>()
                }
            };
        }

        public async Task<IEnumerable<ExpenseListDto>> GetAllExpensesAsync()
        {
            var expenses = await _expenseRepository.GetAllAsync();
            return await MapToExpenseListDtosAsync(expenses);
        }

        public async Task<ExpenseResponseDto> CreateExpenseAsync(CreateExpenseDto createExpenseDto)
        {

            if (!await _groupRepository.ExistsAsync(createExpenseDto.GroupId))
                throw new ArgumentException("Group not found");


            var expense = new Expense
            {
                Description = createExpenseDto.Description,
                Amount = createExpenseDto.Amount,
                Password = createExpenseDto.Password,
                GroupId = createExpenseDto.GroupId
            };

            var createdExpense = await _expenseRepository.CreateAsync(expense);


            var userExpenses = new List<UserExpense>();
            foreach (var detail in createExpenseDto.ExpenseDetails)
            {

                if (!await _userRepository.ExistsAsync(detail.UserId))
                    throw new ArgumentException($"User {detail.UserId} not found");

                if (!await _groupRepository.IsUserInGroupAsync(createExpenseDto.GroupId, detail.UserId))
                    throw new ArgumentException($"User {detail.UserId} is not in the group");

                var userExpenseType = detail.Type.ToUpper() == "PAID_BY"
                    ? UserExpenseType.PAID_BY
                    : UserExpenseType.HEAD_TO_PAY;

                userExpenses.Add(new UserExpense
                {
                    UserId = detail.UserId,
                    ExpenseId = createdExpense.ExpenseId,
                    Amount = detail.Amount,
                    Type = userExpenseType
                });
            }

            await _userExpenseRepository.CreateBulkAsync(userExpenses);


            if (!await ValidateExpenseBalanceAsync(createdExpense.ExpenseId))
                throw new InvalidOperationException("Expense amounts don't balance (total paid != total owed)");

            return await GetExpenseByIdAsync(createdExpense.ExpenseId)
                ?? throw new InvalidOperationException("Failed to retrieve created expense");
        }

        public async Task<ExpenseResponseDto> CreateSimpleExpenseAsync(SimpleExpenseDto simpleExpenseDto)
        {
            try
            {

                if (!await _groupRepository.ExistsAsync(simpleExpenseDto.GroupId))
                    throw new ArgumentException("Group not found");

                if (!await _userRepository.ExistsAsync(simpleExpenseDto.PayerUserId))
                    throw new ArgumentException("Payer user not found");

                if (!await _groupRepository.IsUserInGroupAsync(simpleExpenseDto.GroupId, simpleExpenseDto.PayerUserId))
                    throw new ArgumentException("Payer is not in the group");


                var participantIds = new List<int>();
                if (simpleExpenseDto.ParticipantUserIds != null && simpleExpenseDto.ParticipantUserIds.Any())
                {
                    participantIds.AddRange(simpleExpenseDto.ParticipantUserIds);
                }
                else
                {
                    var groupMembers = await _groupRepository.GetGroupMembersAsync(simpleExpenseDto.GroupId);
                    var memberIds = groupMembers.Select(u => u.UserId).ToList();
                    participantIds.AddRange(memberIds);
                }


                var participantIdsSnapshot = participantIds.ToList();


                foreach (var participantId in participantIdsSnapshot)
                {
                    if (!await _groupRepository.IsUserInGroupAsync(simpleExpenseDto.GroupId, participantId))
                        throw new ArgumentException($"Participant {participantId} is not in the group");
                }


                var participantCount = participantIdsSnapshot.Count;
                var splitAmount = Math.Round(simpleExpenseDto.Amount / participantCount, 2);
                var remainder = simpleExpenseDto.Amount - (splitAmount * participantCount);


                var expenseDetails = new List<CreateExpenseDetailDto>();


                expenseDetails.Add(new CreateExpenseDetailDto
                {
                    UserId = simpleExpenseDto.PayerUserId,
                    Amount = simpleExpenseDto.Amount,
                    Type = "PAID_BY"
                });


                for (int i = 0; i < participantCount; i++)
                {
                    var amount = splitAmount;

                    if (i == 0) amount += remainder;

                    expenseDetails.Add(new CreateExpenseDetailDto
                    {
                        UserId = participantIdsSnapshot[i],
                        Amount = amount,
                        Type = "HEAD_TO_PAY"
                    });
                }

                var createExpenseDto = new CreateExpenseDto
                {
                    Description = simpleExpenseDto.Description,
                    Amount = simpleExpenseDto.Amount,
                    Password = simpleExpenseDto.Password,
                    GroupId = simpleExpenseDto.GroupId,
                    ExpenseDetails = expenseDetails
                };

                return await CreateExpenseAsync(createExpenseDto);
            }
            catch (InvalidOperationException ex)
            {
                throw new ArgumentException($"Collection enumeration error: {ex.Message}");
            }
        }

        public async Task<ExpenseResponseDto?> UpdateExpenseAsync(int id, UpdateExpenseDto updateExpenseDto)
        {
            var expense = await _expenseRepository.GetByIdAsync(id);
            if (expense == null) return null;


            if (!string.IsNullOrEmpty(updateExpenseDto.Description))
                expense.Description = updateExpenseDto.Description;

            if (updateExpenseDto.Amount.HasValue)
                expense.Amount = updateExpenseDto.Amount.Value;

            if (!string.IsNullOrEmpty(updateExpenseDto.Password))
                expense.Password = updateExpenseDto.Password;

            await _expenseRepository.UpdateAsync(expense);


            if (updateExpenseDto.ExpenseDetails != null && updateExpenseDto.ExpenseDetails.Any())
            {

                await _userExpenseRepository.DeleteByExpenseIdAsync(id);


                var userExpenses = new List<UserExpense>();
                foreach (var detail in updateExpenseDto.ExpenseDetails)
                {
                    var userExpenseType = detail.Type.ToUpper() == "PAID_BY"
                        ? UserExpenseType.PAID_BY
                        : UserExpenseType.HEAD_TO_PAY;

                    userExpenses.Add(new UserExpense
                    {
                        UserId = detail.UserId,
                        ExpenseId = id,
                        Amount = detail.Amount ?? 0,
                        Type = userExpenseType
                    });
                }

                await _userExpenseRepository.CreateBulkAsync(userExpenses);


                if (!await ValidateExpenseBalanceAsync(id))
                    throw new InvalidOperationException("Updated expense amounts don't balance");
            }

            return await GetExpenseByIdAsync(id);
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {

            await _userExpenseRepository.DeleteByExpenseIdAsync(id);
            return await _expenseRepository.DeleteAsync(id);
        }


        public async Task<bool> ExpenseExistsAsync(int id)
        {
            return await _expenseRepository.ExistsAsync(id);
        }

        public async Task<bool> ValidateExpensePasswordAsync(int expenseId, string password)
        {
            return await _expenseRepository.ValidateExpensePasswordAsync(expenseId, password);
        }

        public async Task<bool> IsUserAuthorizedForExpenseAsync(int expenseId, int userId)
        {
            return await _expenseRepository.IsUserAuthorizedForExpenseAsync(expenseId, userId);
        }

        public async Task<bool> CanUserModifyExpenseAsync(int expenseId, int userId)
        {
            return await _expenseRepository.CanUserModifyExpenseAsync(expenseId, userId);
        }


        public async Task<ExpenseResponseDto?> GetExpenseWithDetailsAsync(int expenseId)
        {
            return await GetExpenseByIdAsync(expenseId);
        }

        public async Task<ExpenseResponseDto?> GetExpenseWithGroupAsync(int expenseId)
        {
            return await GetExpenseByIdAsync(expenseId);
        }

        public async Task<ExpenseResponseDto?> GetExpenseWithFullDetailsAsync(int expenseId)
        {
            return await GetExpenseByIdAsync(expenseId);
        }


        public async Task<IEnumerable<ExpenseListDto>> GetExpensesByGroupIdAsync(int groupId)
        {
            var expenses = await _expenseRepository.GetExpensesByGroupIdAsync(groupId);
            return await MapToExpenseListDtosAsync(expenses);
        }

        public async Task<IEnumerable<ExpenseListDto>> GetExpensesByUserIdAsync(int userId)
        {
            var expenses = await _expenseRepository.GetExpensesByUserIdAsync(userId);
            return await MapToExpenseListDtosAsync(expenses);
        }

        public async Task<IEnumerable<ExpenseListDto>> GetUserRecentExpensesAsync(int userId, int count = 10)
        {
            var expenses = await _expenseRepository.GetUserRecentExpensesAsync(userId, count);
            return await MapToExpenseListDtosAsync(expenses);
        }

        public async Task<IEnumerable<ExpenseListDto>> GetRecentExpensesAsync(int count = 10)
        {
            var expenses = await _expenseRepository.GetRecentExpensesAsync(count);
            return await MapToExpenseListDtosAsync(expenses);
        }


        public async Task<ExpenseSummaryDto> GetExpenseSummaryAsync(int expenseId)
        {
            var totalPaid = await _expenseRepository.GetExpenseTotalPaidAsync(expenseId);
            var totalToPay = await _expenseRepository.GetExpenseTotalToPayAsync(expenseId);
            var debts = await GetExpenseDebtsAsync(expenseId);

            return new ExpenseSummaryDto
            {
                TotalPaid = totalPaid,
                TotalToPay = totalToPay,
                Debts = debts.ToList()
            };
        }

        public async Task<IEnumerable<DebtDto>> GetExpenseDebtsAsync(int expenseId)
        {
            var debts = await _expenseRepository.GetExpenseDebtsAsync(expenseId);
            var debtDtos = new List<DebtDto>();

            foreach (var debt in debts)
            {
                var debtor = await _userRepository.GetByIdAsync(debt.DebtorId);
                var creditor = await _userRepository.GetByIdAsync(debt.CreditorId);

                if (debtor != null && creditor != null)
                {
                    debtDtos.Add(new DebtDto
                    {
                        DebtorUserId = debt.DebtorId,
                        DebtorName = debtor.Name,
                        CreditorUserId = debt.CreditorId,
                        CreditorName = creditor.Name,
                        Amount = debt.Amount
                    });
                }
            }

            return debtDtos;
        }

        public async Task<Dictionary<int, decimal>> GetExpenseUserBalancesAsync(int expenseId)
        {
            return await _expenseRepository.GetExpenseUserBalancesAsync(expenseId);
        }

        public async Task<decimal> GetExpenseTotalPaidAsync(int expenseId)
        {
            return await _expenseRepository.GetExpenseTotalPaidAsync(expenseId);
        }

        public async Task<decimal> GetExpenseTotalToPayAsync(int expenseId)
        {
            return await _expenseRepository.GetExpenseTotalToPayAsync(expenseId);
        }


        public async Task<bool> ValidateExpenseBalanceAsync(int expenseId)
        {
            return await _userExpenseRepository.ValidateUserExpenseBalanceAsync(expenseId);
        }

        public async Task<ExpenseResponseDto> RecalculateExpenseAsync(int expenseId)
        {

            var expense = await GetExpenseByIdAsync(expenseId);
            if (expense == null) throw new ArgumentException("Expense not found");



            return expense;
        }


        public async Task<IEnumerable<ExpenseListDto>> SearchExpensesByDescriptionAsync(string description)
        {
            var expenses = await _expenseRepository.SearchExpensesByDescriptionAsync(description);
            return await MapToExpenseListDtosAsync(expenses);
        }

        public async Task<IEnumerable<ExpenseListDto>> GetExpensesByAmountRangeAsync(decimal minAmount, decimal maxAmount)
        {
            var expenses = await _expenseRepository.GetExpensesByAmountRangeAsync(minAmount, maxAmount);
            return await MapToExpenseListDtosAsync(expenses);
        }

        public async Task<IEnumerable<ExpenseListDto>> GetExpensesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var expenses = await _expenseRepository.GetExpensesByDateRangeAsync(startDate, endDate);
            return await MapToExpenseListDtosAsync(expenses);
        }


        public async Task<decimal> GetTotalExpensesForUserAsync(int userId)
        {
            return await _expenseRepository.GetTotalExpensesForUserAsync(userId);
        }

        public async Task<decimal> GetTotalExpensesForGroupAsync(int groupId)
        {
            return await _expenseRepository.GetTotalExpensesForGroupAsync(groupId);
        }


        public async Task<bool> SettleExpenseAsync(int expenseId, string password)
        {
            if (!await ValidateExpensePasswordAsync(expenseId, password))
                return false;



            return true;
        }

        public async Task<ExpenseResponseDto> SplitExpenseEquallyAsync(int expenseId, IEnumerable<int> participantIds)
        {
            var expense = await _expenseRepository.GetByIdAsync(expenseId);
            if (expense == null) throw new ArgumentException("Expense not found");

            var participants = participantIds.ToList();
            var splitAmount = Math.Round(expense.Amount / participants.Count, 2);
            var remainder = expense.Amount - (splitAmount * participants.Count);


            await _userExpenseRepository.DeleteByExpenseIdAsync(expenseId);


            var userExpenses = new List<UserExpense>();


            userExpenses.Add(new UserExpense
            {
                UserId = participants[0],
                ExpenseId = expenseId,
                Amount = expense.Amount,
                Type = UserExpenseType.PAID_BY
            });


            for (int i = 0; i < participants.Count; i++)
            {
                var amount = splitAmount;

                if (i == 0) amount += remainder;

                userExpenses.Add(new UserExpense
                {
                    UserId = participants[i],
                    ExpenseId = expenseId,
                    Amount = amount,
                    Type = UserExpenseType.HEAD_TO_PAY
                });
            }

            await _userExpenseRepository.CreateBulkAsync(userExpenses);

            return await GetExpenseByIdAsync(expenseId)
                ?? throw new InvalidOperationException("Failed to retrieve updated expense");
        }


        private async Task<ExpenseResponseDto> MapToResponseDtoAsync(Expense expense)
        {

            var group = await _groupRepository.GetByIdAsync(expense.GroupId);


            var userExpenses = await _userExpenseRepository.GetUserExpensesByExpenseIdAsync(expense.ExpenseId);

            var expenseDetails = new List<ExpenseDetailDto>();
            foreach (var ue in userExpenses)
            {
                var user = await _userRepository.GetByIdAsync(ue.UserId);
                if (user != null)
                {
                    expenseDetails.Add(new ExpenseDetailDto
                    {
                        UserId = ue.UserId,
                        UserName = user.Name,
                        Amount = ue.Amount,
                        Type = ue.Type.ToString()
                    });
                }
            }

            var summary = await GetExpenseSummaryAsync(expense.ExpenseId);

            return new ExpenseResponseDto
            {
                ExpenseId = expense.ExpenseId,
                Description = expense.Description,
                Amount = expense.Amount,
                Group = new ExpenseGroupDto
                {
                    GroupId = expense.GroupId,
                    GroupName = group?.Name ?? "Unknown"
                },
                ExpenseDetails = expenseDetails,
                Summary = summary
            };
        }

        private async Task<IEnumerable<ExpenseListDto>> MapToExpenseListDtosAsync(IEnumerable<Expense> expenses)
        {
            var expenseListDtos = new List<ExpenseListDto>();

            foreach (var expense in expenses)
            {
                var group = expense.Group ?? await _groupRepository.GetByIdAsync(expense.GroupId);
                var paidByUser = expense.UserExpenses?
                    .FirstOrDefault(ue => ue.Type == UserExpenseType.PAID_BY)?.User;

                if (paidByUser == null)
                {
                    var userExpenses = await _userExpenseRepository.GetUserExpensesByExpenseIdAsync(expense.ExpenseId);
                    var paidByUserExpense = userExpenses.FirstOrDefault(ue => ue.Type == UserExpenseType.PAID_BY);
                    if (paidByUserExpense != null)
                    {
                        paidByUser = await _userRepository.GetByIdAsync(paidByUserExpense.UserId);
                    }
                }

                var participantCount = expense.UserExpenses?.Count(ue => ue.Type == UserExpenseType.HEAD_TO_PAY) ?? 0;
                if (participantCount == 0)
                {
                    var userExpenses = await _userExpenseRepository.GetUserExpensesByExpenseIdAsync(expense.ExpenseId);
                    participantCount = userExpenses.Count(ue => ue.Type == UserExpenseType.HEAD_TO_PAY);
                }

                expenseListDtos.Add(new ExpenseListDto
                {
                    ExpenseId = expense.ExpenseId,
                    Description = expense.Description,
                    Amount = expense.Amount,
                    GroupName = group?.Name ?? "Unknown",
                    PaidByUserName = paidByUser?.Name ?? "Unknown",
                    CreatedDate = DateTime.Now, // You might want to add a CreatedDate field to Expense model
                    ParticipantCount = participantCount
                });
            }

            return expenseListDtos;
        }
    }
}