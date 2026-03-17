using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Library.MVC.Models.ViewModels;

public class LoanCreateViewModel
{
    [Required]
    public int BookId { get; set; }

    [Required]
    public int MemberId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime LoanDate { get; set; } = DateTime.Today;

    [Required]
    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

    public List<SelectListItem> AvailableBooks { get; set; } = new();
    public List<SelectListItem> Members { get; set; } = new();
}