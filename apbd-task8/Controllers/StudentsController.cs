namespace apbd_task8.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using apbd_task8.Data;
using apbd_task8.DTOs;
using apbd_task8.Models;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly UniversityTasksDbContext _context;

    public StudentsController(UniversityTasksDbContext context)
    {
        _context = context;
    }

    [HttpGet("{idStudent}/dashboard")]
    public async Task<IActionResult> GetDashboard(int idStudent)
    {
        var dashboard = await _context.Students
            .AsNoTracking()
            .Where(s => s.StudentId == idStudent)
            .Select(s => new StudentDashboardDto
            {
                StudentId = s.StudentId,
                IndexNumber = s.IndexNumber,
                FullName = s.FirstName + " " + s.LastName,
                IsActive = s.IsActive,

                Enrollments = s.Enrollments.Select(e => new EnrollmentInfoDto
                {
                    CourseId = e.CourseId,
                    CourseName = e.Course.Name,
                    Status = e.Status,
                    EnrolledAt = e.EnrolledAt
                }).ToList(),

                Submissions = s.Submissions.Select(sub => new SubmissionInfoDto
                {
                    SubmissionId = sub.SubmissionId,
                    AssignmentTitle = sub.Assignment.Title,
                    RepositoryUrl = sub.RepositoryUrl,
                    Status = sub.Status,
                    Score = sub.Score
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (dashboard == null)
        {
            return NotFound($"Student with id {idStudent} not found.");
        }

        return Ok(dashboard);
    }
}