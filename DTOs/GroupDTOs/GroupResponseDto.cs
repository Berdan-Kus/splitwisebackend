namespace SplitwiseAPI.DTOs.GroupDTOs
{
    public class GroupResponseDto
    {
        public int GroupId { get; set; }
        public string Name { get; set; }

        public List<GroupMemberDto>? Members { get; set; }

        public List<GroupExpenseDto>? Expenses { get; set; }

        public decimal TotalExpenses { get; set; }
        public int MemberCount { get; set; }
    }

    public class GroupMemberDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
    }

    public class GroupExpenseDto
    {
        public int ExpenseId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }

        public string PaidByUserName { get; set; }

        public int ParticipantCount { get; set; }
    }
}