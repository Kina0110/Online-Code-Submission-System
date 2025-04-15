using System;

namespace CodeSubmissionSystem.Models
{
    public class Submission
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string UserId { get; set; }
        public string SourceCode { get; set; }
        public DateTime SubmittedAt { get; set; }
        public int Score { get; set; }
        public string Status { get; set; } // Pending, Completed, Error
        public string FeedbackMessage { get; set; }
        public string TestResults { get; set; } // JSON string of test results
        public virtual Assignment Assignment { get; set; }
        public virtual User User { get; set; }
    }
} 