namespace apbd_task8.DTOs;

public class StudentDashboardDto
{
    public int StudentId { get; set; }
    public string IndexNumber { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public bool IsActive { get; set; }
    public List<EnrollmentInfoDto> Enrollments { get; set; } = new();
    public List<SubmissionInfoDto> Submissions { get; set; } = new();
}

public class EnrollmentInfoDto
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateOnly EnrolledAt { get; set; }
}

public class SubmissionInfoDto
{
    public int SubmissionId { get; set; }
    public string AssignmentTitle { get; set; } = null!;
    public string RepositoryUrl { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int? Score { get; set; }
}