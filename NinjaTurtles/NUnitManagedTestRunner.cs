using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Core;
using NUnit.Core.Filters;
using NUnit.Util;

namespace NinjaTurtles.ManagedTestRunners
{
    class NUnitManagedTestRunner : IManagedTestRunner
    {
        private readonly TestDirectory _testDirectory;
        private readonly string _testAssemblyLocation;
        private readonly string[] _testsToRun;
        private ProxyTestRunner _remoteTestRunner;
        private Task _task;

        public int ExitCode { get; private set; }

        public NUnitManagedTestRunner(TestDirectory testDirectory, string testAssemblyLocation, IEnumerable<string> testsToRun)
        {
            var testAssemblyMutantLocation = Path.Combine(testDirectory.FullName, Path.GetFileName(testAssemblyLocation));

            _testDirectory = testDirectory;
            _testAssemblyLocation = testAssemblyMutantLocation;
            _testsToRun = testsToRun.ToArray();
            _task = new Task(DoTests);
            _remoteTestRunner = new RemoteTestRunner();
            ExitCode = -1;
        }

        public bool Start()
        {
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
            string[] tofilter = _testsToRun;
            TestFilter filter = new SimpleNameFilter(tofilter);
            //TestResult testResult = _remoteTestRunner.Run(new NullListener(), filter, false, LoggingThreshold.Off);
            TestResult testResult = _remoteTestRunner.Run(new NullListener(), TestFilter.Empty, false, LoggingThreshold.Off);
            ExitCode = testResult.IsSuccess ? 0 : 1;
            Console.SetOut(currentOut);
        }
    }
}
