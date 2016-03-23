using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaTurtles.ManagedTestRunners;
using NUnit.Core;

namespace NinjaTurtles.AppDomainIsolation.Adaptor
{
    class NunitManagedTestRunnerAdaptor : MarshalByRefObject
    {
        private readonly NUnitManagedTestRunner _runner = new NUnitManagedTestRunner();

        public bool Start(string testAssemblyLocation)
        {
            return _runner.Start(testAssemblyLocation);
        }

        public bool Start(string testAssemblyLocation, IEnumerable<string> testsToRun)
        {
            return _runner.Start(testAssemblyLocation, testsToRun);
        }

        public void WaitForExit()
        {
            _runner.WaitForExit();
        }

        public bool WaitForExit(int ms)
        {
            return _runner.WaitForExit(ms);
        }

        public TestResult Result
        {
            get { return _runner.Result; }
        }

        public int ExitCode
        {
            get { return _runner.ExitCode; }
        }
    }
}
