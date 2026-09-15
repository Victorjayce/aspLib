using System.ComponentModel.DataAnnotations;

namespace aspLibrary.Models;

public class Book
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Author { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [StringLength(30)]
    [Display(Name = "ISBN")]
    public string? Isbn { get; set; }

    [Range(1, 10000)]
    [Display(Name = "Total copies")]
    public int TotalCopies { get; set; }

    [Display(Name = "Available copies")]
    public int AvailableCopies { get; set; }

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
