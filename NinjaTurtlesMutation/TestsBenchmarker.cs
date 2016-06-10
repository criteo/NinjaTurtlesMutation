using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using NinjaTurtlesMutation.ServiceTestRunnerLib;
using NinjaTurtlesMutation.ServiceTestRunnerLib.Utilities;

namespace NinjaTurtlesMutation
{
    public class TestsBenchmarker : IDisposable
    {
        private const string BENCHMARKER_NAME = "NTMBenchmarker.exe";
        private const int EXIT_MAXWAIT = 1000;

        #region Properties

        private readonly Process _coreProcess;

        #region Pipe COM

        private readonly AnonymousPipeServerStream _pipeIn;
        private readonly AnonymousPipeServerStream _pipeOut;

        #endregion

        #region Stream COM

        private readonly StreamReader _streamIn;
        private readonly StreamWriter _streamOut;

        #endregion

        #endregion

        public TestsBenchmarker()
        {
            _pipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            _pipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            _streamIn = new StreamReader(_pipeIn);
            _streamOut = new StreamWriter(_pipeOut);
            _coreProcess = new Process
            {
                StartInfo =
                {
                    FileName = BENCHMARKER_NAME,
                    UseShellExecute = false,
                    Arguments = _pipeOut.GetClientHandleAsString() + " " +
                                _pipeIn.GetClientHandleAsString()
                }
            };
            _coreProcess.Start();
            _pipeOut.DisposeLocalCopyOfClientHandle();
            _pipeIn.DisposeLocalCopyOfClientHandle();
        }

        public TestDescription Bench(TestDescription toBench)
        {
            TestDescription result = null;
            var watch = Stopwatch.StartNew();
            SendTest(toBench);
            try
            {
                result = ReadATest();
            }
            catch (Exception)
            {
                var elapsedMs = watch.ElapsedMilliseconds;
                result = new TestDescription(toBench);
                result.TestsPass = false;
                result.TotalMsBench = elapsedMs;
            }
            watch.Stop();
            return result;
        }

        #region Tests Exchange Methods

        private void SendTest(TestDescription test)
        {
            TestDescriptionExchanger.SendATestDescription(_streamOut, test);
        }

        private TestDescription ReadATest()
        {
            var test = TestDescriptionExchanger.ReadATestDescription(_streamIn);
            return test;
        }

        #endregion

        #region IDisposable method

        public void Dispose()
        {
            _streamIn.Dispose();
            _streamOut.Dispose();
            _pipeIn.Dispose();
            _pipeOut.Dispose();

            _coreProcess.WaitForExit(EXIT_MAXWAIT);
        }

        #endregion
    }
}
