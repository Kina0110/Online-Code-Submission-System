using System;
using System.Collections.Generic;

namespace CodeSubmissionSystem.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ProgrammingLanguage { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public int MaxScore { get; set; }
        public string TestCases { get; set; } // JSON string of test cases
        public virtual ICollection<Submission> Submissions { get; set; }
    }
} 