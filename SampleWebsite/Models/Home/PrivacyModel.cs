using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SampleWebsite.Models.Home;

public class PrivacyModel
{
    [Required]
    public string? Email { get; set; }

    [Required]
    public int? Month { get; set; }

    public SelectListItem[] MonthList => monthList;

    private static readonly SelectListItem[] monthList = new[] {
        new SelectListItem("January", "1"),
        new SelectListItem("February", "2"),
        new SelectListItem("March", "3"),
        new SelectListItem("April", "4"),
        new SelectListItem("May", "5"),
        new SelectListItem("June", "6"),
        new SelectListItem("July", "7"),
        new SelectListItem("August", "8"),
        new SelectListItem("September", "9"),
        new SelectListItem("October", "10"),
        new SelectListItem("November", "11"),
        new SelectListItem("December", "12"),
    };
}