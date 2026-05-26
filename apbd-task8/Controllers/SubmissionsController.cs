namespace apbd_task8.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using apbd_task8.Data;
using apbd_task8.DTOs;
using apbd_task8.Models;


[ApiController]
[Route("api/submissions")]
public class SubmissionsController : ControllerBase
{
    private readonly UniversityTasksDbContext _context;

    public SubmissionsController(UniversityTasksDbContext context)
    {
        _context = context;
    }


    [HttpPost]
    public async Task<IActionResult> CreateSubmission([FromBody] CreateSubmissionDto dto)
    {
      
        if (!dto.RepositoryUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return BadRequest("RepositoryUrl must start with https://");

        var student = await _context.Students.FindAsync(dto.StudentId);
        if (student == null)
            return NotFound($"Student with id {dto.StudentId} not found.");

        if (!student.IsActive)
            return BadRequest($"Student {student.FirstName} {student.LastName} is not active.");

        
        var assignment = await _context.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.AssignmentId == dto.AssignmentId);

        if (assignment == null)
            return NotFound($"Assignment with id {dto.AssignmentId} not found.");

        if (!assignment.IsPublished)
            return BadRequest("Assignment is not published.");

     
        
        var enrollment = await _context.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                e.StudentId == dto.StudentId &&
                e.CourseId == assignment.CourseId &&
                (e.Status == "Active" || e.Status == "Completed"));

        if (enrollment == null)
            return BadRequest("Student is not enrolled in the course that owns this assignment.");
        
        var alreadySubmitted = await _context.Submissions
            .AnyAsync(s => s.StudentId == dto.StudentId && s.AssignmentId == dto.AssignmentId);

        if (alreadySubmitted)
            return Conflict("Student has already submitted this assignment.");

        
        var now = DateTime.UtcNow;
        var status = assignment.DueDate < now ? "Late" : "Submitted";

        var submission = new Submission
        {
            AssignmentId = dto.AssignmentId,
            StudentId = dto.StudentId,
            RepositoryUrl = dto.RepositoryUrl,
            SubmittedAt = now,
            Status = status,
            Score = null,
            Feedback = null
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSubmission), new { idSubmission = submission.SubmissionId },
            new { submission.SubmissionId, submission.Status, submission.SubmittedAt });
    }

    [HttpPut("{idSubmission}/grade")]
    public async Task<IActionResult> GradeSubmission(int idSubmission, [FromBody] GradeSubmissionDto dto)
    {
        var submission = await _context.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.SubmissionId == idSubmission);

        if (submission == null)
            return NotFound($"Submission with id {idSubmission} not found.");

        if (dto.Score > submission.Assignment.MaxPoints)
            return BadRequest($"Score {dto.Score} exceeds MaxPoints {submission.Assignment.MaxPoints}.");

        submission.Score = dto.Score;
        submission.Feedback = dto.Feedback;
        submission.Status = "Graded";

        await _context.SaveChangesAsync();

        return Ok(new
        {
            submission.SubmissionId,
            submission.Score,
            submission.Feedback,
            submission.Status
        });
    }

    [HttpDelete("{idSubmission}")]
    public async Task<IActionResult> DeleteSubmission(int idSubmission)
    {
        var submission = await _context.Submissions.FindAsync(idSubmission);

        if (submission == null)
            return NotFound($"Submission with id {idSubmission} not found.");

        if (submission.Status == "Graded")
            return BadRequest("Cannot delete a graded submission.");

        _context.Submissions.Remove(submission);
        await _context.SaveChangesAsync();

        return NoContent(); 
    }

    [HttpGet("{idSubmission}")]
    public async Task<IActionResult> GetSubmission(int idSubmission)
    {
        var submission = await _context.Submissions
            .AsNoTracking()
            .Include(s => s.Student)
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.SubmissionId == idSubmission);

        if (submission == null)
            return NotFound();

        return Ok(new SubmissionDto
        {
            SubmissionId = submission.SubmissionId,
            StudentName = $"{submission.Student.FirstName} {submission.Student.LastName}",
            AssignmentTitle = submission.Assignment.Title,
            RepositoryUrl = submission.RepositoryUrl,
            Status = submission.Status,
            Score = submission.Score,
            Feedback = submission.Feedback,
            SubmittedAt = submission.SubmittedAt
        });
    }
}