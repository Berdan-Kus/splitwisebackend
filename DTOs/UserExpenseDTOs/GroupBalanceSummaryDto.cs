namespace SplitwiseAPI.DTOs.UserExpenseDTOs
{
    public class GroupBalanceSummaryDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public decimal TotalExpenses { get; set; }
        public List<UserBalanceSummaryDto> UserBalances { get; set; }
        public List<SimplifiedDebtDto> SimplifiedDebts { get; set; }
    }

    public class UserBalanceSummaryDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOwed { get; set; }
        public decimal NetBalance { get; set; }
    }

    public class SimplifiedDebtDto
    {
        public int FromUserId { get; set; }
        public string FromUserName { get; set; }
        public int ToUserId { get; set; }
        public string ToUserName { get; set; }
        public decimal Amount { get; set; }
    }

    public class SettlementHistoryDto
    {
        public int SettlementId { get; set; }
        public int DebtorUserId { get; set; }
        public string DebtorUserName { get; set; }
        public int CreditorUserId { get; set; }
        public string CreditorUserName { get; set; }
        public decimal Amount { get; set; }
        public DateTime SettlementDate { get; set; }
        public string? Note { get; set; }
        public string GroupName { get; set; }
    }

    public class UserDashboardDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }

        public decimal TotalNetBalance { get; set; }
        public int ActiveGroupsCount { get; set; }
        public int TotalExpensesCount { get; set; }

        public List<RecentActivityDto> RecentActivities { get; set; }

        public List<ActiveDebtDto> ActiveDebts { get; set; }
    }

    public class RecentActivityDto
    {
        public int ExpenseId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string GroupName { get; set; }
        public string ActivityType { get; set; }
        public DateTime Date { get; set; }
    }

    public class ActiveDebtDto
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public int ExpenseCount { get; set; }
    }
}