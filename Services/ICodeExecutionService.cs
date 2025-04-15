using System.Threading.Tasks;
using CodeSubmissionSystem.Models;

namespace CodeSubmissionSystem.Services
{
    public interface ICodeExecutionService
    {
        Task<(bool success, string output, string error)> ExecuteCodeAsync(string sourceCode, string language);
        Task<(int score, string feedback)> EvaluateSubmissionAsync(Submission submission);
    }
} 