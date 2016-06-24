using System;

namespace NinjaTurtlesMutation.ManagedTestRunners
{
    interface IManagedTestRunner: IDisposable
    {
        /// <summary>
        /// Runs the tests associate with the current instance asynchronously
        /// </summary>
        /// <returns>True if the tests run without failures, false otherwise</returns>
        bool Start();

        /// <summary>
        /// Instruct the current instance to wait for the associated run to exit
        /// </summary>
        /// <returns></returns>
        void WaitForExit();
        
        /// <summary>
        /// Instruct the current instance to wait the specified number of milliseconds for the associated run to exit
        /// </summary>
        /// <param name="ms"></param>
        /// <returns>True if the run ended in the specified number of milliseconds, false otherwise</returns>
        bool WaitForExit(int ms);
    }
}
