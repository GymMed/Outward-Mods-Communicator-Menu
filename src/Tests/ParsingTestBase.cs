using System;
using BepInEx.Logging;

namespace OutwardModsCommunicatorMenu.Tests
{
    public abstract class ParsingTestBase
    {
        protected ManualLogSource Logger { get; }
        protected ITestResultAggregator ResultAggregator { get; }

        protected ParsingTestBase() : this(OMCM.Log, new TestResultAggregator(OMCM.Log))
        {
        }

        protected ParsingTestBase(ManualLogSource logger, ITestResultAggregator resultAggregator)
        {
            Logger = logger;
            ResultAggregator = resultAggregator;
        }

        protected void LogTestStart(string testName)
        {
            Logger?.LogInfo($"[TEST] Starting: {testName}");
        }

        protected void LogTestEnd(string testName, bool passed)
        {
            Logger?.LogInfo($"[TEST] {(passed ? "PASSED" : "FAILED")}: {testName}");
            if (passed)
                ResultAggregator?.RecordPass(testName);
            else
                ResultAggregator?.RecordFail(testName, "");
        }

        protected void LogValue<T>(string label, T value)
        {
            Logger?.LogInfo($"[TEST]   {label}: {value}");
        }

        protected void LogError(string message)
        {
            Logger?.LogError($"[TEST] ERROR: {message}");
        }

        protected void Assert<T>(bool condition, string message = "")
        {
            if (!condition)
            {
                throw new Exception($"Assertion failed: {message}");
            }
        }

        protected void AssertBool(bool condition, string message = "")
        {
            if (!condition)
            {
                throw new Exception($"Assertion failed: {message}");
            }
        }

        protected void AssertEqual<T>(T expected, T actual, string context = "")
        {
            string contextStr = string.IsNullOrEmpty(context) ? "" : $" ({context})";
            if (!Equals(expected, actual))
            {
                throw new Exception($"Assertion failed{contextStr}: expected {expected}, got {actual}");
            }
        }

        protected void AssertNotNull(object value, string context = "")
        {
            string contextStr = string.IsNullOrEmpty(context) ? "" : $" ({context})";
            if (value == null)
            {
                throw new Exception($"Assertion failed{contextStr}: expected non-null value");
            }
        }

        protected void AssertNull(object value, string context = "")
        {
            string contextStr = string.IsNullOrEmpty(context) ? "" : $" ({context})";
            if (value != null)
            {
                throw new Exception($"Assertion failed{contextStr}: expected null, got {value}");
            }
        }

        protected void RunTest(string testName, Action testAction)
        {
            LogTestStart(testName);
            try
            {
                testAction();
                LogTestEnd(testName, true);
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                LogTestEnd(testName, false);
            }
        }

        protected void RunTestWithResult<T>(string testName, Func<(T Value, string Error)> testFunc, Action<T> verifyAction)
        {
            LogTestStart(testName);
            try
            {
                var (value, error) = testFunc();
                if (error != null)
                {
                    LogError($"Parse error: {error}");
                    LogTestEnd(testName, false);
                    return;
                }
                
                verifyAction(value);
                LogTestEnd(testName, true);
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                LogTestEnd(testName, false);
            }
        }
    }
}
