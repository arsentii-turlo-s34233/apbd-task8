namespace apbd_task8.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using apbd_task8.Data;
using apbd_task8.DTOs;
[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly UniversityTasksDbContext _context;
    public CoursesController(UniversityTasksDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetCourses([FromQuery] bool activeOnly = false)
    {
        var query = _context.Courses.AsNoTracking();
        if(activeOnly)
            query = query.Where(c=> c.IsActive);
        var courses = await query.Select(c => new CourseDto
        {
            CourseId = c.CourseId,
            Code = c.Code,
            Name = c.Name,
            Credits = c.Credits,
            AssignmentCount = c.Assignments.Count()
        }).ToListAsync();
        return Ok(courses);
    }

    [HttpGet("{idCourse}/assignments")]
    public async Task<IActionResult> GetAssignments(int idCourse, [FromQuery] bool publishedOnly = false)
    {
        var courseExists = await _context.Courses.AsNoTracking().AnyAsync(c => c.CourseId == idCourse);
        if (!courseExists)
            return NotFound($"Course with id {idCourse} not found.");
        var query = _context.Assignments.AsNoTracking().Where(a => a.CourseId == idCourse);
        if (publishedOnly)
            query = query.Where(a => a.IsPublished);
        var assignments = await query.Select(a => new AssignmentDto
        {
            AssignmentId = a.AssignmentId,
            Title = a.Title,
            DueDate = a.DueDate,
            MaxPoints = a.MaxPoints,
            IsPublished = a.IsPublished,
            SubmissionCount = a.Submissions.Count()
        }).ToListAsync();
        return Ok(assignments);
    }
}