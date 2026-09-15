using System.ComponentModel.DataAnnotations;

namespace aspLibrary.Models;

public class Member
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Date registered")]
    public DateTime DateRegistered { get; set; } = DateTime.Today;

    [Display(Name = "Active member")]
    public bool IsActive { get; set; } = true;

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
