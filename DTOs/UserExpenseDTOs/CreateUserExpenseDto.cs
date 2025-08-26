using System.ComponentModel.DataAnnotations;

namespace SplitwiseAPI.DTOs.UserExpenseDTOs
{
    public class CreateUserExpenseDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Expense ID is required")]
        public int ExpenseId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount cannot be negative")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [RegularExpression("^(PAID_BY|HEAD_TO_PAY)$", ErrorMessage = "Type must be either 'PAID_BY' or 'HEAD_TO_PAY'")]
        public string Type { get; set; }
    }

    // Toplu UserExpense oluşturma için
    public class CreateBulkUserExpenseDto
    {
        [Required(ErrorMessage = "Expense ID is required")]
        public int ExpenseId { get; set; }

        [Required(ErrorMessage = "User expenses are required")]
        [MinLength(1, ErrorMessage = "At least one user expense is required")]
        public List<UserExpenseItemDto> UserExpenses { get; set; }
    }

    public class UserExpenseItemDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount cannot be negative")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [RegularExpression("^(PAID_BY|HEAD_TO_PAY)$", ErrorMessage = "Type must be either 'PAID_BY' or 'HEAD_TO_PAY'")]
        public string Type { get; set; }
    }
}