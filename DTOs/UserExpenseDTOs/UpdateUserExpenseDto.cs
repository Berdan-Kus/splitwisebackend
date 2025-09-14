using System.ComponentModel.DataAnnotations;

namespace SplitwiseAPI.DTOs.UserExpenseDTOs
{
    public class UpdateUserExpenseDto
    {
        [Range(0, double.MaxValue, ErrorMessage = "Amount cannot be negative")]
        public decimal? Amount { get; set; }

        [RegularExpression("^(PAID_BY|HEAD_TO_PAY)$", ErrorMessage = "Type must be either 'PAID_BY' or 'HEAD_TO_PAY'")]
        public string? Type { get; set; }
    }

    public class SettleDebtDto
    {
        [Required(ErrorMessage = "Debtor user ID is required")]
        public int DebtorUserId { get; set; }

        [Required(ErrorMessage = "Creditor user ID is required")]
        public int CreditorUserId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [StringLength(255, ErrorMessage = "Note cannot exceed 255 characters")]
        public string? Note { get; set; }
    }
}