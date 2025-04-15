using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CodeSubmissionSystem.Data;
using CodeSubmissionSystem.Models;
using CodeSubmissionSystem.Services;
using System.Security.Claims;

namespace CodeSubmissionSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubmissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICodeExecutionService _codeExecutionService;

        public SubmissionsController(ApplicationDbContext context, ICodeExecutionService codeExecutionService)
        {
            _context = context;
            _codeExecutionService = codeExecutionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Submission>>> GetSubmissions()
        {
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            if (user.Role == "Student")
            {
                return await _context.Submissions
                    .Include(s => s.Assignment)
                    .Where(s => s.UserId == userId)
                    .ToListAsync();
            }
            
            return await _context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.User)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Submission>> GetSubmission(int id)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(User.Identity.Name);
            if (user.Role == "Student" && submission.UserId != user.Id)
            {
                return Forbid();
            }

            return submission;
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<Submission>> CreateSubmission(Submission submission)
        {
            var assignment = await _context.Assignments.FindAsync(submission.AssignmentId);
            if (assignment == null)
            {
                return BadRequest("Assignment not found");
            }

            if (DateTime.UtcNow > assignment.DueDate)
            {
                return BadRequest("Assignment due date has passed");
            }

            var userId = User.FindFirst(ClaimTypes.Name)?.Value;
            submission.UserId = userId;
            submission.SubmittedAt = DateTime.UtcNow;
            submission.Status = "Pending";

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            // Start evaluation in background
            _ = EvaluateSubmissionAsync(submission.Id);

            return CreatedAtAction(nameof(GetSubmission), new { id = submission.Id }, submission);
        }

        private async Task EvaluateSubmissionAsync(int submissionId)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            try
            {
                var (score, feedback) = await _codeExecutionService.EvaluateSubmissionAsync(submission);
                
                submission.Score = score;
                submission.Status = "Completed";
                submission.FeedbackMessage = feedback;
                
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                submission.Status = "Error";
                submission.FeedbackMessage = ex.Message;
                await _context.SaveChangesAsync();
            }
        }

        [HttpGet("assignment/{assignmentId}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<IEnumerable<Submission>>> GetSubmissionsByAssignment(int assignmentId)
        {
            return await _context.Submissions
                .Include(s => s.User)
                .Where(s => s.AssignmentId == assignmentId)
                .ToListAsync();
        }
    }
} 