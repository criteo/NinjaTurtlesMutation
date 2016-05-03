using System;
using System.Collections.Generic;
using System.Text;

namespace NinjaTurtles.ServiceTestRunnerLib
{
    /// <summary>
    /// This class describe a test run
    /// </summary>
    public class TestDescription
    {
        /// <summary>
        /// The uid of this test description
        /// </summary>
        public string Uid;

        /// <summary>
        /// The path to the test assembly
        /// </summary>
        public string AssemblyPath;

        /// <summary>
        /// The list of the tests to run
        /// </summary>
        public List<string> TestsToRun;

        /// <summary>
        /// How long the this test take without mutation
        /// </summary>
        public long TotalMsBench;

        /// <summary>
        /// Does the run finish in time
        /// </summary>
        public bool ExitedInTime;

        /// <summary>
        /// Does the tests pass
        /// </summary>
        public bool TestsPass;

        /// <summary>
        /// Empty constructor, mostly here for the xml serialization
        /// </summary>
        public TestDescription()
        {
            Uid = Guid.NewGuid().ToString("N");
            AssemblyPath = null;
            TestsToRun = null;
            TotalMsBench = -1;
            ExitedInTime = false;
            TestsPass = false;
        }

        /// <summary>
        /// Instantiate a test description, usually use from the server
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <param name="testsToRun"></param>
        /// <param name="totalMsBench"></param>
        public TestDescription(string assemblyPath, IEnumerable<string> testsToRun, long totalMsBench) : this()
        {
            AssemblyPath = assemblyPath;
            TestsToRun = new List<string>(testsToRun);
            TotalMsBench = totalMsBench;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Test {0}, bench {1}ms", Uid, TotalMsBench);
            return sb.ToString();
        }
    }
}
