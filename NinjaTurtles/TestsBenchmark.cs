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

namespace NinjaTurtles
{
    class TestsBenchmark
    {
        private readonly string _testAssemblyLocation;

        public long TotalMs { get; private set; }
        public IEnumerable<string> testsName;

        public TestsBenchmark(string testAssemblyLocation)
        {
            TotalMs = -1;
            _testAssemblyLocation = testAssemblyLocation;
        }

        public TestsBenchmark(string testAssemblyLocation, IEnumerable<string> testsName) : this(testAssemblyLocation)
        {
            this.testsName = testsName;
        }

        public long LaunchBenchmark()
        {
            TotalMs = FullSetBench();
            return TotalMs;
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
            }
            return elapsedMs;
        }

        public override string ToString()
        {
            return "Bench total ms:" + TotalMs;
        }
    }
}
