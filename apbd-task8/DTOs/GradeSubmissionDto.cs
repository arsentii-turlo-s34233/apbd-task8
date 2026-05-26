using System.ComponentModel.DataAnnotations;

namespace apbd_task8.DTOs;

public class GradeSubmissionDto
{
    [Required]
    [Range(0, int.MaxValue)]
    public int Score { get; set; }

    public string? Feedback { get; set; }
}