using System;
using Microsoft.AspNetCore.Identity;

namespace CodeSubmissionSystem.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; } // Student, Instructor, Admin
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<Assignment> CreatedAssignments { get; set; }
        public virtual ICollection<Submission> Submissions { get; set; }
    }
} 