using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SplitwiseAPI.Models
{
    public class UserExpense
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserExpenseId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public UserExpenseType Type { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ExpenseId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ExpenseId")]
        public virtual Expense Expense { get; set; }
    }
}