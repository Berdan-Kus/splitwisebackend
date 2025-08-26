namespace SplitwiseAPI.DTOs.ExpenseDTOs
{
    public class ExpenseResponseDto
    {
        public int ExpenseId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        // Password'u response'a dahil etmiyoruz

        // Grup bilgisi
        public ExpenseGroupDto Group { get; set; }

        // Masraf detayları
        public List<ExpenseDetailDto> ExpenseDetails { get; set; }

        // Hesaplanmış bilgiler
        public ExpenseSummaryDto Summary { get; set; }
    }

    public class ExpenseGroupDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }

    public class ExpenseDetailDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } // "PAID_BY" veya "HEAD_TO_PAY"
    }

    public class ExpenseSummaryDto
    {
        public decimal TotalPaid { get; set; }
        public decimal TotalToPay { get; set; }
        public List<DebtDto> Debts { get; set; }
    }

    // Borç hesaplaması
    public class DebtDto
    {
        public int DebtorUserId { get; set; }
        public string DebtorName { get; set; }
        public int CreditorUserId { get; set; }
        public string CreditorName { get; set; }
        public decimal Amount { get; set; }
    }
}