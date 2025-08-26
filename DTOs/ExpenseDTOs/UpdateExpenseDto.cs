using System.ComponentModel.DataAnnotations;

namespace SplitwiseAPI.DTOs.ExpenseDTOs
{
    public class UpdateExpenseDto
    {
        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; }

        [StringLength(255, ErrorMessage = "Password cannot exceed 255 characters")]
        public string? Password { get; set; }

        // Masraf detaylarını güncellemek için
        public List<UpdateExpenseDetailDto>? ExpenseDetails { get; set; }
    }

    public class UpdateExpenseDetailDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Amount cannot be negative")]
        public decimal? Amount { get; set; }

        [Required(ErrorMessage = "Expense type is required")]
        public string Type { get; set; } // "PAID_BY" veya "HEAD_TO_PAY"
    }
}