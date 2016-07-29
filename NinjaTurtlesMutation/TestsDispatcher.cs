using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using NinjaTurtlesMutation.ServiceTestRunnerLib;
using NinjaTurtlesMutation.ServiceTestRunnerLib.Utilities;

namespace NinjaTurtlesMutation
{
    public class TestsDispatcher : IDisposable
    {
        private const string DISPATCHER_NAME = "NTMDispatcher.exe";
        private const int EXIT_MAXWAIT = 1000;

        #region Properties

        private readonly Process _coreProcess;

        #region Pipe COM

        private readonly AnonymousPipeServerStream _pipeIn;
        private readonly AnonymousPipeServerStream _pipeOut;
        private readonly AnonymousPipeServerStream _pipeCmd;

        #endregion

        #region Stream COM

        private readonly StreamReader _streamIn;
        private readonly StreamWriter _streamOut;
        private readonly StreamWriter _streamCmd;

        #endregion

        #region Tests Exchange

        private long _testsSended;
        private long _testsReceived;

        public bool TestsPending {
            get { return _testsSended > _testsReceived; }
        }

        #endregion

        #endregion

        public TestsDispatcher(int parallelLevel, int maxBusyRunners, bool oneTimeRunners, float killTimeFactor)
        {
            _pipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            _pipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            _pipeCmd = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            _streamIn = new StreamReader(_pipeIn);
            _streamOut = new StreamWriter(_pipeOut);
            _streamCmd = new StreamWriter(_pipeCmd);
            _coreProcess = new Process
            {
                StartInfo =
                {
                    FileName = DISPATCHER_NAME,
                    UseShellExecute = false,
                    Arguments = _pipeOut.GetClientHandleAsString() + " " +
                                _pipeIn.GetClientHandleAsString() + " " +
                                _pipeCmd.GetClientHandleAsString() + " " +
                                parallelLevel + " " +
                                maxBusyRunners + " " +
                                oneTimeRunners + " " +
                                killTimeFactor
                }
            };
            _coreProcess.Start();
            _pipeCmd.DisposeLocalCopyOfClientHandle();
            _pipeOut.DisposeLocalCopyOfClientHandle();
            _pipeIn.DisposeLocalCopyOfClientHandle();
        }

        #region Tests Exchange Methods

        public void SendTest(TestDescription test)
        {
            TestDescriptionExchanger.SendATestDescription(_streamOut, test);
            _testsSended++;
        }

        public TestDescription ReadATest()
        {
            var test = TestDescriptionExchanger.ReadATestDescription(_streamIn);
            _testsReceived++;
            return test;
        }

        #endregion

        #region IDisposable method

        public void Dispose()
        {
            CommandExchanger.SendData(_streamCmd, CommandExchanger.Commands.STOP);

            _streamIn.Dispose();
            _streamOut.Dispose();
            _streamCmd.Dispose();
            _pipeIn.Dispose();
            _pipeOut.Dispose();
            _pipeCmd.Dispose();

            _coreProcess.WaitForExit(EXIT_MAXWAIT);
        }

        #endregion
    }
}
