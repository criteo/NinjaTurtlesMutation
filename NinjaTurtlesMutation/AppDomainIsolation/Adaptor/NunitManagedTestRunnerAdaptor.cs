using System;
using System.Collections.Generic;
using NinjaTurtlesMutation.ManagedTestRunners;
using NUnit.Core;

namespace NinjaTurtlesMutation.AppDomainIsolation.Adaptor
{
    public class NunitManagedTestRunnerAdaptor : Adaptor
    {
        private readonly NUnitManagedTestRunner _runner = new NUnitManagedTestRunner();

        public override bool Start(string testAssemblyLocation)
        {
            return _runner.Start(testAssemblyLocation);
        }

        public override bool Start(string testAssemblyLocation, IEnumerable<string> testsToRun)
        {
            return _runner.Start(testAssemblyLocation, testsToRun);
        }

        public override void WaitForExit()
        {
            _runner.WaitForExit();
        }

        public override bool WaitForExit(int ms)
        {
            return _runner.WaitForExit(ms);
        }

        public TestResult Result
        {
            get { return _runner.Result; }
        }

        public override int ExitCode
        {
            get { return _runner.ExitCode; }
        }

        public override bool IsCompleted()
        {
            return _runner.IsCompleted();
        }

        public override void Dispose()
        {
            _runner.Dispose();
        }
    }
}
