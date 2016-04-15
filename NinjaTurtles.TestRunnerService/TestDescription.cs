﻿using System.Collections.Generic;

namespace NinjaTurtles.TestRunnerService
{
    /// <summary>
    /// This class describe a test run
    /// </summary>
    public class TestDescription
    {
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
        public TestDescription(string assemblyPath, List<string> testsToRun, long totalMsBench) : this()
        {
            AssemblyPath = assemblyPath;
            TestsToRun = testsToRun;
            TotalMsBench = totalMsBench;
        }
    }
}
