using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Core;
using NUnit.Core.Filters;
using NUnit.Util;

namespace NinjaTurtles.ManagedTestRunners
{
    class NUnitManagedTestRunner : IManagedTestRunner
    {
        public int ExitCode;
        public TestResult Result;
        
        private string[] _testsToRun;
        private string _testAssemblyLocation;
        private readonly RemoteTestRunner _remoteTestRunner;
        private Task _task;

        public NUnitManagedTestRunner()
        {
            _testAssemblyLocation = null;
            _testsToRun = new string[0];
            Result = null;
            _remoteTestRunner = new RemoteTestRunner();
            ExitCode = -1;
        }

        public NUnitManagedTestRunner(TestDirectory testDirectory, string testAssemblyLocation, IEnumerable<string> testsToRun)
        {
            var testAssemblyMutantLocation = Path.Combine(testDirectory.FullName, Path.GetFileName(testAssemblyLocation));

            _testAssemblyLocation = testAssemblyMutantLocation;
            _testsToRun = testsToRun.ToArray();
            _remoteTestRunner = new RemoteTestRunner();
            Result = null;
            ExitCode = -1;
        }

        public NUnitManagedTestRunner(string testAssemblyLocation)
        {
            _testAssemblyLocation = testAssemblyLocation;
            _testsToRun = new string[0];
            _remoteTestRunner = new RemoteTestRunner();
            Result = null;
            ExitCode = -1;
        }

        public bool Start()
        {
            _task = new Task(DoTests);
            _task.Start();
            return true;
        }

        public bool Start(string testAssemblyLocation)
        {
            _testAssemblyLocation = testAssemblyLocation;
            _task = new Task(DoTests);
            _task.Start();
            return true;
        }

        public bool Start(string testAssemblyLocation, IEnumerable<string> testsToRun)
        {
            _testAssemblyLocation = testAssemblyLocation;
            _testsToRun = testsToRun.ToArray();
            _task = new Task(DoTests);
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
            Thread.Sleep(ms);
            if (ExitCode != -1)
                return (true);
            _remoteTestRunner.CancelRun();
            return (false);
        }

        private void DoTests()
        {
            var currentOut = Console.Out;
            TestPackage testPackage = new TestPackage(_testAssemblyLocation);

            _remoteTestRunner.Load(testPackage);
            TestExecutionContext.CurrentContext.TestPackage.Settings.Add("StopOnError", true);
            if (_testsToRun.Length > 0)
            {
                string[] tofilter = _testsToRun;
                TestFilter filter = new SimpleNameFilter(tofilter);
                Result = _remoteTestRunner.Run(new NullListener(), filter, false, LoggingThreshold.Off);
            }
            else
                Result = _remoteTestRunner.Run(new NullListener(), TestFilter.Empty, false, LoggingThreshold.Off);
            ExitCode = Result.IsSuccess ? 0 : 1;
            Console.SetOut(currentOut);
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
    }
}
