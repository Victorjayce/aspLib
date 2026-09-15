using System.ComponentModel.DataAnnotations;

namespace aspLibrary.Models;

public enum LoanStatus { Borrowed, Returned }

public class Loan
{
    public int Id { get; set; }

    [Required, Display(Name = "Book")]
    public int BookId { get; set; }
    public Book? Book { get; set; }

    [Required, Display(Name = "Member")]
    public int MemberId { get; set; }
    public Member? Member { get; set; }

    [Display(Name = "Borrowed date")]
    [DataType(DataType.Date)]
    public DateTime BorrowedDate { get; set; } = DateTime.Today;

    [Display(Name = "Due date")]
    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

    [Display(Name = "Returned date")]
    [DataType(DataType.Date)]
    public DateTime? ReturnedDate { get; set; }

    public LoanStatus Status { get; set; } = LoanStatus.Borrowed;

    public bool IsOverdue => Status == LoanStatus.Borrowed && DueDate.Date < DateTime.Today;
}
