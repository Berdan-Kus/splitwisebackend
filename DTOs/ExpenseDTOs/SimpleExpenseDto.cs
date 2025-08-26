using System.ComponentModel.DataAnnotations;

namespace SplitwiseAPI.DTOs.ExpenseDTOs
{
    // Basit masraf oluşturma - eşit bölüştürme için
    public class SimpleExpenseDto
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

        [Required(ErrorMessage = "Payer user ID is required")]
        public int PayerUserId { get; set; }

        // Masrafı paylaşacak kullanıcılar (boş ise grup üyelerinin tümü)
        public List<int>? ParticipantUserIds { get; set; }
    }

    // Masraf listesi için basit DTO
    public class ExpenseListDto
    {
        public int ExpenseId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string GroupName { get; set; }
        public string PaidByUserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ParticipantCount { get; set; }
    }
}