namespace SplitwiseAPI.DTOs.UserExpenseDTOs
{
    public class UserExpenseResponseDto
    {
        public int UserExpenseId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }

        public UserExpenseUserDto User { get; set; }

        public UserExpenseExpenseDto Expense { get; set; }
    }

    public class UserExpenseUserDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
    }

    public class UserExpenseExpenseDto
    {
        public int ExpenseId { get; set; }
        public string Description { get; set; }
        public decimal TotalAmount { get; set; }
        public string GroupName { get; set; }
    }

    public class UserBalanceDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOwed { get; set; }
        public decimal NetBalance { get; set; }

        public List<UserDebtDetailDto> DebtDetails { get; set; }
    }

    public class UserDebtDetailDto
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public List<DebtExpenseDto> RelatedExpenses { get; set; }
    }

    public class DebtExpenseDto
    {
        public int ExpenseId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string GroupName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}