using System.ComponentModel.DataAnnotations;

namespace SplitwiseAPI.DTOs.ExpenseDTOs
{
    public class CreateExpenseDto
    {
        [Required(ErrorMessage = "Description is required")]
        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, ErrorMessage = "Password cannot exceed 255 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Group ID is required")]
        public int GroupId { get; set; }

        [Required(ErrorMessage = "Expense details are required")]
        [MinLength(1, ErrorMessage = "At least one expense detail is required")]
        public List<CreateExpenseDetailDto> ExpenseDetails { get; set; }
    }

    public class CreateExpenseDetailDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount cannot be negative")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Expense type is required")]
        public string Type { get; set; }
    }
}