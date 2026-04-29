using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MVCCore.Models;

public class IndexRequestModel
{
    public string text { get; set; } = "";

    [Required]
    public IFormFile? file1 { get; set; }

    [Required]
    public IFormFile? file2 { get; set; }
}