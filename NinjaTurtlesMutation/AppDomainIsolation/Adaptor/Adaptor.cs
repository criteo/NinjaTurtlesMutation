using System;
using System.Collections.Generic;

namespace NinjaTurtlesMutation.AppDomainIsolation.Adaptor
{
    /// <summary>
    /// Abstract class used by Isolated to interact with a runner
    /// </summary>
    public abstract class Adaptor : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Start the tests designated by testAssemblyLocation
        /// </summary>
        /// <param name="testAssemblyLocation"></param>
        /// <returns></returns>
        public abstract bool Start(string testAssemblyLocation);

        /// <summary>
        /// Start the tests designated by testAssemblyLocation only with
        /// the one specified in testsToRun
        /// </summary>
        /// <param name="testAssemblyLocation"></param>
        /// <param name="testsToRun"></param>
        /// <returns></returns>
        public abstract bool Start(string testAssemblyLocation, IEnumerable<string> testsToRun);

        /// <summary>
        /// Block until the tests end
        /// </summary>
        public abstract void WaitForExit();

        /// <summary>
        /// Block ms milliseconds and return true if the tests run ended
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public abstract bool WaitForExit(int ms);

        /// <summary>
        /// Exitcode of the test runner
        /// </summary>
        public abstract int ExitCode { get; }

        /// <summary>
        /// Return true if the underlying Task of the test runner has ended
        /// </summary>
        /// <returns></returns>
        public abstract bool IsCompleted();

        public abstract void Dispose();
    }
}
