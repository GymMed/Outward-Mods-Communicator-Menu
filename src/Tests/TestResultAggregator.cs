using System.Text;
using BepInEx.Logging;

namespace OutwardModsCommunicatorMenu.Tests
{
    public class TestResultAggregator : ITestResultAggregator
    {
        private readonly ManualLogSource _logger;
        private int _passedCount;
        private int _failedCount;

        public int PassedCount => _passedCount;
        public int FailedCount => _failedCount;

        public TestResultAggregator(ManualLogSource logger)
        {
            _logger = logger;
        }

        public void RecordPass(string testName)
        {
            _passedCount++;
        }

        public void RecordFail(string testName, string error)
        {
            _failedCount++;
        }

        public void PrintSummary()
        {
            _logger?.LogInfo($"[TEST] === SUMMARY: Passed: {_passedCount}, Failed: {_failedCount} ===");
        }
    }
}
