using System.Collections.Generic;
using System.Diagnostics;
using NinjaTurtlesMutation.AppDomainIsolation;
using NinjaTurtlesMutation.AppDomainIsolation.Adaptor;
using NinjaTurtlesMutation.ServiceTestRunnerLib;

namespace NinjaTurtlesMutation
{
    class TestsBenchmark
    {
        private readonly string _testAssemblyLocation;

        public long TotalMs { get; private set; }
        public bool TestsPass { get; private set; }
        public ICollection<string> testsName;

        public TestsBenchmark(string testAssemblyLocation)
        {
            TotalMs = -1;
            TestsPass = false;
            _testAssemblyLocation = testAssemblyLocation;
            testsName = new List<string>();
        }

        public TestsBenchmark(string testAssemblyLocation, ICollection<string> testsName) : this(testAssemblyLocation)
        {
            this.testsName = testsName;
        }

        public long LaunchBenchmark(bool useSeparatedProcess = false)
        {
            if (useSeparatedProcess)
                TotalMs = SeparatedProcessBench();
            else if (testsName.Count > 0)
                TotalMs = FullSetBench();
            else
                TotalMs = NoSetBench();
            return TotalMs;
        }

        private long SeparatedProcessBench()
        {
            TestDescription toBench = new TestDescription(_testAssemblyLocation, testsName, -1);
            TestDescription benchedTest = null;
            using (TestsBenchmarker benchProc = new TestsBenchmarker())
            {
                benchedTest = benchProc.Bench(toBench);
            }
            TestsPass = benchedTest.TestsPass;
            return benchedTest.TotalMsBench;
        }

        private long FullSetBench()
        {
            long elapsedMs = 0;
            using (Isolated<NunitManagedTestRunnerAdaptor> runner = new Isolated<NunitManagedTestRunnerAdaptor>())
            {
                var watch = Stopwatch.StartNew();
                runner.Instance.Start(_testAssemblyLocation, testsName);
                runner.Instance.WaitForExit();
                watch.Stop();
                elapsedMs = watch.ElapsedMilliseconds;
                TestsPass = (runner.Instance.ExitCode == 0);
            }
            return elapsedMs;
        }

        private long NoSetBench()
        {
            long elapsedMs = 0;
            using (Isolated<NunitManagedTestRunnerAdaptor> runner = new Isolated<NunitManagedTestRunnerAdaptor>())
            {
                var watch = Stopwatch.StartNew();
                runner.Instance.Start(_testAssemblyLocation);
                runner.Instance.WaitForExit();
                watch.Stop();
                elapsedMs = watch.ElapsedMilliseconds;
                TestsPass = (runner.Instance.ExitCode == 0);
            }
            return elapsedMs;
        }

        public override string ToString()
        {
            return "Bench total ms:" + TotalMs;
        }
    }
}
