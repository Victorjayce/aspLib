using aspLibrary.Models;

namespace aspLibrary.ViewModels;

public class DashboardViewModel
{
    public int TotalBooks { get; set; }
    public int AvailableCopies { get; set; }
    public int BorrowedCopies { get; set; }
    public int ActiveMembers { get; set; }
    public int OverdueLoans { get; set; }
    public List<Loan> RecentLoans { get; set; } = [];
    public List<Loan> OverdueLoanList { get; set; } = [];
}
