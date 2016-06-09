using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaTurtlesMutation.AppDomainIsolation;
using NinjaTurtlesMutation.AppDomainIsolation.Adaptor;
using NinjaTurtlesMutation.ServiceTestRunnerLib;
using NinjaTurtlesMutation.ServiceTestRunnerLib.Utilities;

namespace NinjaTurtlesMutation.Benchmarker
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
                Environment.Exit(1);
            using (PipeStream receivePipe = new AnonymousPipeClientStream(PipeDirection.In, args[0]))
            using (PipeStream sendPipe = new AnonymousPipeClientStream(PipeDirection.Out, args[1]))
            using (StreamWriter sendStream = new StreamWriter(sendPipe))
            using (StreamReader receiveStream = new StreamReader(receivePipe))
            using (new ErrorModeContext(ErrorModes.FailCriticalErrors | ErrorModes.NoGpFaultErrorBox))
            {
                try
                {
                    var testDescription = TestDescriptionExchanger.ReadATestDescription(receiveStream);
                    Console.WriteLine("Benchmarker receive:\n{0}", testDescription); ////////
                    RunDescribedTests(testDescription);
                    TestDescriptionExchanger.SendATestDescription(sendStream, testDescription);
                    Console.WriteLine("Benchmarker sended:\n{0}", testDescription); ////////
                }
                catch (Exception)
                {
                    Environment.Exit(2);
                }
            }
            Environment.Exit(0);
        }

        private static void RunDescribedTests(TestDescription testDescription)
        {
            if (testDescription.TestsToRun.Count == 0)
                RunAllTests(testDescription);
            else
                RunSpecifiedTests(testDescription);
        }

        private static void RunAllTests(TestDescription testDescription)
        {
            int exitCode;
            Console.WriteLine("Null TestsToRun");///////////////
            using (Isolated<NunitManagedTestRunnerAdaptor> runner = new Isolated<NunitManagedTestRunnerAdaptor>())
            {
                var mutantPath = testDescription.AssemblyPath;
                var watch = Stopwatch.StartNew();
                runner.Instance.Start(mutantPath);
                runner.Instance.WaitForExit();
                watch.Stop();
                testDescription.TotalMsBench = watch.ElapsedMilliseconds;
                exitCode = runner.Instance.ExitCode;
            }
            testDescription.TestsPass = (exitCode == 0);
        }

        private static void RunSpecifiedTests(TestDescription testDescription)
        {
            int exitCode;
            Console.WriteLine("Not Null TestsToRun");///////////////
            using (Isolated<NunitManagedTestRunnerAdaptor> runner = new Isolated<NunitManagedTestRunnerAdaptor>())
            {
                var mutantPath = testDescription.AssemblyPath;
                var watch = Stopwatch.StartNew();
                runner.Instance.Start(mutantPath, testDescription.TestsToRun);
                runner.Instance.WaitForExit();
                watch.Stop();
                testDescription.TotalMsBench = watch.ElapsedMilliseconds;
                exitCode = runner.Instance.ExitCode;
            }
            testDescription.TestsPass = (exitCode == 0);
        }
    }
}
