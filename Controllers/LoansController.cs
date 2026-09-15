using aspLibrary.Data;
using aspLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace aspLibrary.Controllers;

public class LoansController(LibraryDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? status)
    {
        var query = context.Loans.Include(l => l.Book).Include(l => l.Member).AsQueryable();
        var today = DateTime.Today;
        if (status == "Borrowed") query = query.Where(l => l.Status == LoanStatus.Borrowed && l.DueDate >= today);
        if (status == "Returned") query = query.Where(l => l.Status == LoanStatus.Returned);
        if (status == "Overdue") query = query.Where(l => l.Status == LoanStatus.Borrowed && l.DueDate < today);
        ViewBag.Status = status;
        return View(await query.OrderByDescending(l => l.BorrowedDate).ToListAsync());
    }

    public async Task<IActionResult> Overdue() => View("Index", await context.Loans.Include(l => l.Book).Include(l => l.Member)
        .Where(l => l.Status == LoanStatus.Borrowed && l.DueDate < DateTime.Today).OrderBy(l => l.DueDate).ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var loan = await context.Loans.Include(l => l.Book).Include(l => l.Member).FirstOrDefaultAsync(l => l.Id == id);
        return loan is null ? NotFound() : View(loan);
    }

    public async Task<IActionResult> Create()
    {
        await SetSelectLists();
        return View(new Loan { BorrowedDate = DateTime.Today, DueDate = DateTime.Today.AddDays(14) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Loan loan)
    {
        var book = await context.Books.FindAsync(loan.BookId);
        var member = await context.Members.FindAsync(loan.MemberId);
        if (book is null) ModelState.AddModelError(nameof(loan.BookId), "Select a valid book.");
        else if (book.AvailableCopies < 1) ModelState.AddModelError(nameof(loan.BookId), "This book has no available copies.");
        if (member is null || !member.IsActive) ModelState.AddModelError(nameof(loan.MemberId), "Select an active member.");
        if (loan.DueDate.Date < loan.BorrowedDate.Date) ModelState.AddModelError(nameof(loan.DueDate), "Due date must be on or after the borrowed date.");
        if (!ModelState.IsValid) { await SetSelectLists(); return View(loan); }
        loan.Status = LoanStatus.Borrowed; loan.ReturnedDate = null;
        book!.AvailableCopies--;
        context.Loans.Add(loan); await context.SaveChangesAsync();
        TempData["Success"] = "Loan created and book availability updated."; return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(int id)
    {
        var loan = await context.Loans.Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == id);
        if (loan is null) return NotFound();
        if (loan.Status == LoanStatus.Returned) { TempData["Error"] = "This loan has already been returned."; return RedirectToAction(nameof(Details), new { id }); }
        loan.Status = LoanStatus.Returned; loan.ReturnedDate = DateTime.Today;
        loan.Book!.AvailableCopies++;
        await context.SaveChangesAsync(); TempData["Success"] = "Book returned and availability updated."; return RedirectToAction(nameof(Details), new { id });
    }

    private async Task SetSelectLists()
    {
        ViewBag.Books = new SelectList(await context.Books.Where(b => b.AvailableCopies > 0).OrderBy(b => b.Title).ToListAsync(), "Id", "Title");
        ViewBag.Members = new SelectList(await context.Members.Where(m => m.IsActive).OrderBy(m => m.FullName).ToListAsync(), "Id", "FullName");
    }
}
