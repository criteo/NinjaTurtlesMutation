using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
            var processTotal = GetTestRunnerProcess(testDir, testsMethods);
            var totalMsWatch = Stopwatch.StartNew();
            
            processTotal.Start();
            processTotal.WaitForExit();
            totalMsWatch.Stop();
            TotalMs = totalMsWatch.ElapsedMilliseconds;
            Parallel.ForEach(testsMethods, new ParallelOptions {MaxDegreeOfParallelism = -1},
                testMethod => IndividualMethodBench(testDir, testMethod));
            return TotalMs;
        }

        private long IndividualMethodBench(TestDirectory testDirectory, string testMethod)
        {
            var testToRun = new List<string>() {testMethod};
            var process = GetTestRunnerProcess(testDirectory, testToRun);
            var watch = Stopwatch.StartNew();

            process.Start();
            process.WaitForExit();
            watch.Stop();
            MethodsBenchIDictionary[testMethod] = watch.ElapsedMilliseconds;
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

    }
}
