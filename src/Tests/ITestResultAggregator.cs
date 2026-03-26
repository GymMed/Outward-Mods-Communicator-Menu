namespace OutwardModsCommunicatorMenu.Tests
{
    public interface ITestResultAggregator
    {
        int PassedCount { get; }
        int FailedCount { get; }
        void RecordPass(string testName);
        void RecordFail(string testName, string error);
        void PrintSummary();
    }
}
