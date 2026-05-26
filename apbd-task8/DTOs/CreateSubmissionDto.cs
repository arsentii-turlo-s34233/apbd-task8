using System.ComponentModel.DataAnnotations;

namespace apbd_task8.DTOs;

public class CreateSubmissionDto
{
    [Required]
    public int AssignmentId { get; set; }

    [Required]
    public int StudentId { get; set; }

    [Required]
    [Url]
    public string RepositoryUrl { get; set; } = null!;
}