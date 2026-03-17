using System.ComponentModel.DataAnnotations;

namespace Library.Domain.Models;

public class Loan
{
    public int Id { get; set; }

    [Required]
    public int BookId { get; set; }

    [Required]
    public int MemberId { get; set; }

    [DataType(DataType.Date)]
    public DateTime LoanDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ReturnedDate { get; set; }

    public Book? Book { get; set; }
    public Member? Member { get; set; }
    
    public bool IsActive => ReturnedDate == null;

    public bool IsOverdue => ReturnedDate == null && DueDate.Date < DateTime.Today;
}