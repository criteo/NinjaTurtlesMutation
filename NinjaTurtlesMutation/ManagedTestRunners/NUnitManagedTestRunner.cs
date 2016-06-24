using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Core;
using NUnit.Core.Filters;
using NUnit.Util;

namespace NinjaTurtlesMutation.ManagedTestRunners
{
    public class NUnitManagedTestRunner : IManagedTestRunner
    {
        public int ExitCode;
        public TestResult Result;
        
        private string[] _testsToRun;
        private string _testAssemblyLocation;
        private readonly RemoteTestRunner _remoteTestRunner;
        private Task _task;

        private const double POLL_TIME_FACTOR = 0.1;

        public NUnitManagedTestRunner()
        {
            _task = new Task(DoTests);
            _testAssemblyLocation = null;
            _testsToRun = null;
            Result = null;
            _remoteTestRunner = new RemoteTestRunner();
            ExitCode = -1;
        }

        public NUnitManagedTestRunner(string testAssemblyLocation) : this()
        {
            _testAssemblyLocation = testAssemblyLocation;
            InitAndLoadTestPackage();
        }

        public NUnitManagedTestRunner(TestDirectory testDirectory, string testAssemblyLocation, IEnumerable<string> testsToRun) : this(Path.Combine(testDirectory.FullName, Path.GetFileName(testAssemblyLocation)))
        {
            _testsToRun = testsToRun.ToArray();
        }

        private void InitAndLoadTestPackage()
        {
            if (_testAssemblyLocation == null)
                throw new Exception("No test assembly to load");
            TestPackage testPackage = new TestPackage(_testAssemblyLocation);

            _remoteTestRunner.Load(testPackage);
            TestExecutionContext.CurrentContext.TestPackage.Settings.Add("StopOnError", true);
        }

        public bool Start()
        {
            _task.Start();
            return true;
        }

        public bool Start(string testAssemblyLocation)
        {
            _testAssemblyLocation = testAssemblyLocation;
            InitAndLoadTestPackage();
            _task.Start();
            return true;
        }

        public bool Start(string testAssemblyLocation, IEnumerable<string> testsToRun)
        {
            _testAssemblyLocation = testAssemblyLocation;
            _testsToRun = testsToRun.ToArray();
            InitAndLoadTestPackage();
            _task.Start();
            return true;
        }

        public void WaitForExit()
        {
            _task.Wait();
        }

        public bool WaitForExit(int ms)
        {
            if (ExitCode != -1)
                return (true);
            TimedExitCodePolling(ms, ms * POLL_TIME_FACTOR);
            if (ExitCode != -1)
                return (true);
            Task.Factory.StartNew(() => _remoteTestRunner.CancelRun());
            return (false);
        }

        private void TimedExitCodePolling(int maxWaitMs, double pollSleepMs)
        {
            int pollSleepIntMs = (int)Math.Ceiling(pollSleepMs);
            var watch = Stopwatch.StartNew();

            while (watch.ElapsedMilliseconds < maxWaitMs)
            {
                Thread.Sleep(pollSleepIntMs);
                if (ExitCode != -1)
                    return;
            }
        }

        private void DoTests()
        {
            var currentOut = Console.Out;

            TestFilter filter = (_testsToRun != null ? new SimpleNameFilter(_testsToRun) : TestFilter.Empty);
            Result = _remoteTestRunner.Run(new NullListener(), filter, false, LoggingThreshold.Off);
            ExitCode = Result.IsSuccess ? 0 : 1;
            Console.SetOut(currentOut);
            //WriteErrorsAndFailuresReport(Result);
        }

        public bool IsCompleted()
        {
            return _task.IsCompleted;
        }

        public static void WriteSummaryReport(TestResult result)
        {
            var summary = new ResultSummarizer(result);
            Console.WriteLine(
                "Tests run: {0}, Errors: {1}, Failures: {2}, Inconclusive: {3}, Time: {4} seconds",
                summary.TestsRun, summary.Errors, summary.Failures, summary.Inconclusive, summary.Time);
            Console.WriteLine(
                "  Not run: {0}, Invalid: {1}, Ignored: {2}, Skipped: {3}",
                summary.TestsNotRun, summary.NotRunnable, summary.Ignored, summary.Skipped);
            Console.WriteLine();

            Console.WriteLine("IsSuccess: {0}", result.IsSuccess);
        }

        public void Dispose()
        {
            _remoteTestRunner.Dispose();
        }
        
         /*private static int reportIndex;

        private void WriteErrorsAndFailuresReport(TestResult result)
        {
            reportIndex = 0;
            Console.WriteLine("Errors and Failures:");
            WriteErrorsAndFailures(result);
            Console.WriteLine();
        }

        private void WriteErrorsAndFailures(TestResult result)
        {
            if (result.Executed)
            {
                if (result.HasResults)
                {
                    if (result.IsFailure || result.IsError)
                        if (result.FailureSite == FailureSite.SetUp || result.FailureSite == FailureSite.TearDown)
                            WriteSingleResult(result);

                    foreach (TestResult childResult in result.Results)
                        WriteErrorsAndFailures(childResult);
                }
                else if (result.IsFailure || result.IsError)
                {
                    WriteSingleResult(result);
                }
            }
        }

        private void WriteSingleResult(TestResult result)
        {
            string status = result.IsFailure || result.IsError
                ? string.Format("{0} {1}", result.FailureSite, result.ResultState)
                : result.ResultState.ToString();

            Console.WriteLine("{0}) {1} : {2}", ++reportIndex, status, result.FullName);

            if (result.Message != null && result.Message != string.Empty)
                Console.WriteLine("   {0}", result.Message);

            if (result.StackTrace != null && result.StackTrace != string.Empty)
                Console.WriteLine(result.IsFailure
                    ? StackTraceFilter.Filter(result.StackTrace)
                    : result.StackTrace + Environment.NewLine);
        }*/
    }
}
