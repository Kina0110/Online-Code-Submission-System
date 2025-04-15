using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CodeSubmissionSystem.Data;
using CodeSubmissionSystem.Models;
using System.Security.Claims;

namespace CodeSubmissionSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssignmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AssignmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Assignment>>> GetAssignments()
        {
            return await _context.Assignments
                .Include(a => a.CreatedBy)
                .Where(a => a.IsActive)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Assignment>> GetAssignment(int id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.CreatedBy)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            return assignment;
        }

        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<Assignment>> CreateAssignment(Assignment assignment)
        {
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            assignment.CreatedById = userId;
            assignment.CreatedAt = DateTime.UtcNow;
            assignment.IsActive = true;

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAssignment), new { id = assignment.Id }, assignment);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> UpdateAssignment(int id, Assignment assignment)
        {
            if (id != assignment.Id)
            {
                return BadRequest();
            }

            var userId = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var existingAssignment = await _context.Assignments.FindAsync(id);
            if (existingAssignment == null)
            {
                return NotFound();
            }

            if (existingAssignment.CreatedById != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            existingAssignment.Title = assignment.Title;
            existingAssignment.Description = assignment.Description;
            existingAssignment.ProgrammingLanguage = assignment.ProgrammingLanguage;
            existingAssignment.DueDate = assignment.DueDate;
            existingAssignment.MaxScore = assignment.MaxScore;
            existingAssignment.TestCases = assignment.TestCases;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssignmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            if (assignment.CreatedById != User.Identity.Name && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            assignment.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AssignmentExists(int id)
        {
            return _context.Assignments.Any(e => e.Id == id);
        }
    }
} 