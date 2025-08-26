namespace SplitwiseAPI.DTOs.UserDTOs
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }

        // Password'u response'a dahil etmiyoruz (güvenlik)

        // İsteğe bağlı navigation properties
        public List<UserGroupResponseDto>? Groups { get; set; }
        public List<UserExpenseDetailDto>? Expenses { get; set; }
    }

    // Basit grup bilgisi (circular reference'ı önlemek için)
    public class UserGroupResponseDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }

    // Basit masraf bilgisi - ismi değiştirildi çakışmayı önlemek için
    public class UserExpenseDetailDto
    {
        public int ExpenseId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } // PAID_BY veya HEAD_TO_PAY
    }
}