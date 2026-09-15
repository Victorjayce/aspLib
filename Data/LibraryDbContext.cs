using aspLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace aspLibrary.Data;

public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>().Property(b => b.Title).HasMaxLength(200);
        modelBuilder.Entity<Member>().HasIndex(m => m.Email).IsUnique();
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Book).WithMany(b => b.Loans).HasForeignKey(l => l.BookId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Member).WithMany(m => m.Loans).HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
