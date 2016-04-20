using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaTurtles.AppDomainIsolation;
using NinjaTurtles.AppDomainIsolation.Adaptor;
using NinjaTurtles.ServiceTestRunnerLib;
using NinjaTurtles.ServiceTestRunnerLib.Utilities;

namespace NinjaTurtles.TestRunnerService
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
                return;
            try
            {
                using (PipeStream receivePipe = new AnonymousPipeClientStream(PipeDirection.In, args[0]))
                using (PipeStream sendPipe = new AnonymousPipeClientStream(PipeDirection.Out, args[1]))
                using (StreamWriter sendStream = new StreamWriter(sendPipe))
                using (StreamReader receiveStream = new StreamReader(receivePipe))
                {
                    while (true)
                    {
                        var testDescription = TestDescriptionExchanger.ReadATestDescription(receiveStream);
                        RunDescribedTests(testDescription);
                        TestDescriptionExchanger.SendATestDescription(sendStream, testDescription);
                    }
                }
            }
            catch (IOException)
            {
                Console.Error.WriteLine("[TESTRUNNER] Broken pipe");
                Environment.ExitCode = 1;
            }
            catch
            {
                Console.Error.WriteLine("[TESTRUNNER] Crash");
                Environment.ExitCode = 2;
            }
        }

        private static void RunDescribedTests(TestDescription testDescription)
        {
            bool exitedInTime;
            int exitCode;

            using (Isolated<NunitManagedTestRunnerAdaptor> runner = new Isolated<NunitManagedTestRunnerAdaptor>())
            {
                var mutantPath = testDescription.AssemblyPath;
                runner.Instance.Start(mutantPath, testDescription.TestsToRun);
                exitedInTime = runner.Instance.WaitForExit((int) (1.1 * testDescription.TotalMsBench));
                exitCode = runner.Instance.ExitCode;
            }

            /*var mutantPath = testDescription.AssemblyPath;
            var runner = new NUnitManagedTestRunner();
            runner.Start(mutantPath, testDescription.TestsToRun);
            exitedInTime = runner.WaitForExit((int)(1.1 * testDescription.TotalMsBench));
            exitCode = runner.ExitCode;*/
            testDescription.ExitedInTime = exitedInTime;
            testDescription.TestsPass = (exitCode == 0);
        }
    }
}
