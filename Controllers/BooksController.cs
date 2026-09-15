using aspLibrary.Data;
using aspLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aspLibrary.Controllers;

public class BooksController(LibraryDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search, string? category)
    {
        var query = context.Books.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search) || (b.Isbn ?? "").Contains(search));
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(b => b.Category == category);
        ViewBag.Search = search;
        ViewBag.Category = category;
        ViewBag.Categories = await context.Books.Select(b => b.Category).Distinct().OrderBy(c => c).ToListAsync();
        return View(await query.OrderBy(b => b.Title).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var book = await context.Books.Include(b => b.Loans).ThenInclude(l => l.Member).FirstOrDefaultAsync(b => b.Id == id);
        return book is null ? NotFound() : View(book);
    }

    public IActionResult Create() => View(new Book());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Book book)
    {
        if (!ModelState.IsValid) return View(book);
        book.AvailableCopies = book.TotalCopies;
        context.Books.Add(book);
        await context.SaveChangesAsync();
        TempData["Success"] = "Book added to the catalogue.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var book = await context.Books.FindAsync(id);
        return book is null ? NotFound() : View(book);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Book book)
    {
        if (id != book.Id) return NotFound();
        var existing = await context.Books.FindAsync(id);
        if (existing is null) return NotFound();
        var borrowed = existing.TotalCopies - existing.AvailableCopies;
        if (book.TotalCopies < borrowed) ModelState.AddModelError(nameof(book.TotalCopies), $"Total copies cannot be lower than {borrowed}, the number currently on loan.");
        if (!ModelState.IsValid) { book.AvailableCopies = existing.AvailableCopies; return View(book); }
        existing.Title = book.Title; existing.Author = book.Author; existing.Category = book.Category; existing.Isbn = book.Isbn;
        existing.TotalCopies = book.TotalCopies; existing.AvailableCopies = book.TotalCopies - borrowed;
        await context.SaveChangesAsync();
        TempData["Success"] = "Book updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var book = await context.Books.Include(b => b.Loans).FirstOrDefaultAsync(b => b.Id == id);
        return book is null ? NotFound() : View(book);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await context.Books.Include(b => b.Loans).FirstOrDefaultAsync(b => b.Id == id);
        if (book is null) return NotFound();
        if (book.Loans.Any()) { TempData["Error"] = "A book with loan history cannot be deleted."; return RedirectToAction(nameof(Details), new { id }); }
        context.Books.Remove(book); await context.SaveChangesAsync();
        TempData["Success"] = "Book deleted."; return RedirectToAction(nameof(Index));
    }
}
