using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using aspLibrary.Models;
using aspLibrary.Data;
using aspLibrary.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace aspLibrary.Controllers;

public class HomeController(LibraryDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var loans = context.Loans.Include(l => l.Book).Include(l => l.Member);
        return View(new DashboardViewModel
        {
            TotalBooks = await context.Books.CountAsync(),
            AvailableCopies = await context.Books.SumAsync(b => (int?)b.AvailableCopies) ?? 0,
            BorrowedCopies = await context.Loans.CountAsync(l => l.Status == LoanStatus.Borrowed),
            ActiveMembers = await context.Members.CountAsync(m => m.IsActive),
            OverdueLoans = await loans.CountAsync(l => l.Status == LoanStatus.Borrowed && l.DueDate < today),
            RecentLoans = await loans.OrderByDescending(l => l.BorrowedDate).Take(6).ToListAsync(),
            OverdueLoanList = await loans.Where(l => l.Status == LoanStatus.Borrowed && l.DueDate < today)
                .OrderBy(l => l.DueDate).Take(5).ToListAsync()
        });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
