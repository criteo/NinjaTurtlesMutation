using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NinjaTurtles.AppDomainIsolation;
using NinjaTurtles.AppDomainIsolation.Adaptor;
using NinjaTurtles.TestRunners;

namespace NinjaTurtles
{
    class TestsBenchmark
    {
        private ITestRunner _runner;
        private readonly string _testAssemblyLocation;

        public long TotalMs { get; private set; }
        public IDictionary<string, long> MethodsBenchIDictionary { get; private set; }

        public TestsBenchmark(string testAssemblyLocation)
        {
            TotalMs = -1;
            MethodsBenchIDictionary = new ConcurrentDictionary<string, long>();
            _testAssemblyLocation = testAssemblyLocation;
        }

        public TestsBenchmark(string testAssemblyLocation, IEnumerable<string> testsMethods) : this(testAssemblyLocation)
        {
            foreach (var testMethod in testsMethods)
                MethodsBenchIDictionary[testMethod] = -1;
        }

        public IList<string> GetTestsName()
        {
            return new List<string>(MethodsBenchIDictionary.Keys);
        }

        public void AddTest(string methodName)
        {
            if (MethodsBenchIDictionary.Keys.Contains(methodName))
                return;
            MethodsBenchIDictionary.Add(methodName, -1);
        }

        public long LaunchBenchmark()
        {
            IList<string> testsMethods = GetTestsName();
            var testDir = new TestDirectory(Path.GetDirectoryName(_testAssemblyLocation));
            TotalMs = FullSetBench();
            foreach (var testMethod in testsMethods)
                IndividualMethodBench(testDir, testMethod);
            /*Parallel.ForEach(testsMethods, new ParallelOptions {MaxDegreeOfParallelism = -1},
                testMethod => IndividualMethodBench(testDir, testMethod));*/
            return TotalMs;
        }

        private long FullSetBench()
        {
            long elapsedMs = 0;
            using (Isolated<NunitManagedTestRunnerAdaptor> runner = new Isolated<NunitManagedTestRunnerAdaptor>())
            {
                var watch = Stopwatch.StartNew();
                runner.Instance.Start(_testAssemblyLocation);
                runner.Instance.WaitForExit();
                watch.Stop();
                elapsedMs = watch.ElapsedMilliseconds;
            }
            return elapsedMs;
        }

        private long IndividualMethodBench(TestDirectory testDirectory, string testMethod)
        {
            var testToRun = new List<string>() {testMethod};
            long elapsedMs = 0;
            using (Isolated<NunitManagedTestRunnerAdaptor> runner = new Isolated<NunitManagedTestRunnerAdaptor>())
            {
                var watch = Stopwatch.StartNew();
                runner.Instance.Start(_testAssemblyLocation, testToRun);
                runner.Instance.WaitForExit();
                watch.Stop();
                elapsedMs = watch.ElapsedMilliseconds;
            }
            MethodsBenchIDictionary[testMethod] = elapsedMs;
            return MethodsBenchIDictionary[testMethod];
        }

        private Process GetTestRunnerProcess(TestDirectory testDirectory, IList<string> testsMethods)
        {
            if (_runner == null)
            {
                _runner = (ITestRunner)Activator.CreateInstance(MutationTestBuilder.TestRunner);
                _runner.EnsureRunner(testDirectory, _testAssemblyLocation);
            }
            return _runner.GetRunnerProcess(testDirectory, _testAssemblyLocation, testsMethods);
        }

        public override string ToString()
        {
            var fullReport = "";
            foreach (var bench in MethodsBenchIDictionary)
                fullReport += "\n" + bench.Key + ": " + bench.Value;
            return "Total ms:" + TotalMs + fullReport;
        }
    }
}
