namespace SplitwiseAPI.DTOs.GroupDTOs
{
    public class GroupResponseDto
    {
        public int GroupId { get; set; }
        public string Name { get; set; }

        // Grup üyeleri
        public List<GroupMemberDto>? Members { get; set; }

        // Grup masrafları
        public List<GroupExpenseDto>? Expenses { get; set; }

        // Grup özeti bilgileri
        public decimal TotalExpenses { get; set; }
        public int MemberCount { get; set; }
    }

    // Basit üye bilgisi
    public class GroupMemberDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
    }

    // Basit masraf bilgisi
    public class GroupExpenseDto
    {
        public int ExpenseId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }

        // Kim ödedi
        public string PaidByUserName { get; set; }

        // Masraf detayları sayısı
        public int ParticipantCount { get; set; }
    }
}