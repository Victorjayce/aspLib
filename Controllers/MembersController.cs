using aspLibrary.Data;
using aspLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aspLibrary.Controllers;

public class MembersController(LibraryDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var query = context.Members.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(m => m.FullName.Contains(search) || m.Email.Contains(search) || m.PhoneNumber.Contains(search));
        ViewBag.Search = search;
        return View(await query.OrderByDescending(m => m.IsActive).ThenBy(m => m.FullName).ToListAsync());
    }
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var member = await context.Members.Include(m => m.Loans).ThenInclude(l => l.Book).FirstOrDefaultAsync(m => m.Id == id);
        return member is null ? NotFound() : View(member);
    }
    public IActionResult Create() => View(new Member());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Member member)
    {
        if (await context.Members.AnyAsync(m => m.Email == member.Email)) ModelState.AddModelError(nameof(member.Email), "This email address is already registered.");
        if (!ModelState.IsValid) return View(member);
        member.DateRegistered = DateTime.Today; context.Add(member); await context.SaveChangesAsync();
        TempData["Success"] = "Member registered."; return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Edit(int? id) => id is null ? NotFound() : (await context.Members.FindAsync(id) is Member member ? View(member) : NotFound());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Member member)
    {
        if (id != member.Id) return NotFound();
        if (await context.Members.AnyAsync(m => m.Email == member.Email && m.Id != id)) ModelState.AddModelError(nameof(member.Email), "This email address is already registered.");
        if (!ModelState.IsValid) return View(member);
        context.Update(member); await context.SaveChangesAsync(); TempData["Success"] = "Member updated."; return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var member = await context.Members.FindAsync(id); if (member is null) return NotFound();
        member.IsActive = false; await context.SaveChangesAsync(); TempData["Success"] = "Member deactivated."; return RedirectToAction(nameof(Details), new { id });
    }
}
