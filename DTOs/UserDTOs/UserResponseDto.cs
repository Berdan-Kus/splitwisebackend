namespace SplitwiseAPI.DTOs.UserDTOs
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }


        public List<UserGroupResponseDto>? Groups { get; set; }
        public List<UserExpenseDetailDto>? Expenses { get; set; }
    }

    public class UserGroupResponseDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }

    public class UserExpenseDetailDto
    {
        public int ExpenseId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
    }
}