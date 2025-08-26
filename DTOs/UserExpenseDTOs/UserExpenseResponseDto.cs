namespace SplitwiseAPI.DTOs.UserExpenseDTOs
{
    public class UserExpenseResponseDto
    {
        public int UserExpenseId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } // "PAID_BY" veya "HEAD_TO_PAY"

        // User bilgisi
        public UserExpenseUserDto User { get; set; }

        // Expense bilgisi
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

    // Kullanıcı bakiye özeti
    public class UserBalanceDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOwed { get; set; }
        public decimal NetBalance { get; set; } // Pozitif: alacaklı, Negatif: borçlu

        // Detaylı borç/alacak listesi
        public List<UserDebtDetailDto> DebtDetails { get; set; }
    }

    public class UserDebtDetailDto
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } // "OWE" (borçlusun) veya "OWED" (alacaklısın)
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