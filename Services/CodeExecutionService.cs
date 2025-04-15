using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CodeSubmissionSystem.Models;

namespace CodeSubmissionSystem.Services
{
    public class CodeExecutionService : ICodeExecutionService
    {
        private readonly string _containerName = "code-execution";
        private readonly string _tempDirectory;

        public CodeExecutionService()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "CodeSubmissions");
            Directory.CreateDirectory(_tempDirectory);
        }

        public async Task<(bool success, string output, string error)> ExecuteCodeAsync(string sourceCode, string language)
        {
            var fileName = $"{Guid.NewGuid()}.{GetFileExtension(language)}";
            var filePath = Path.Combine(_tempDirectory, fileName);

            try
            {
                await File.WriteAllTextAsync(filePath, sourceCode);
                var (success, output, error) = await RunInDockerContainer(filePath, language);
                return (success, output, error);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        public async Task<(int score, string feedback)> EvaluateSubmissionAsync(Submission submission)
        {
            var testCases = JsonSerializer.Deserialize<TestCase[]>(submission.Assignment.TestCases);
            var totalScore = 0;
            var feedback = "";

            foreach (var testCase in testCases)
            {
                var (success, output, error) = await ExecuteCodeAsync(submission.SourceCode, submission.Assignment.ProgrammingLanguage);
                
                if (!success)
                {
                    return (0, $"Execution error: {error}");
                }

                if (output.Trim() == testCase.ExpectedOutput.Trim())
                {
                    totalScore += testCase.Points;
                    feedback += $"Test case {testCase.Id}: Passed (+{testCase.Points} points)\n";
                }
                else
                {
                    feedback += $"Test case {testCase.Id}: Failed\nExpected: {testCase.ExpectedOutput}\nGot: {output}\n";
                }
            }

            return (totalScore, feedback);
        }

        private async Task<(bool success, string output, string error)> RunInDockerContainer(string filePath, string language)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"run --rm -v {filePath}:/code/{Path.GetFileName(filePath)} {_containerName}-{language}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return (process.ExitCode == 0, output, error);
        }

        private string GetFileExtension(string language) => language.ToLower() switch
        {
            "python" => "py",
            "java" => "java",
            "c#" => "cs",
            "javascript" => "js",
            _ => throw new ArgumentException($"Unsupported language: {language}")
        };
    }

    public class TestCase
    {
        public int Id { get; set; }
        public string Input { get; set; }
        public string ExpectedOutput { get; set; }
        public int Points { get; set; }
    }
} 