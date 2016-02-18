using System;

namespace NinjaTurtles.TestRunners
{
    /// <summary>
    /// TestRunnerException is used to convey problems that arise when using the test runners.
    /// </summary>
    [Serializable]
    public class TestRunnerException : Exception
    {
        /// <summary>
        /// Create a TestRunnerException with a message decribing the problem
        /// </summary>
        /// <param name="message">The message</param>
        public TestRunnerException(string message) : base(message) { }
    }
}