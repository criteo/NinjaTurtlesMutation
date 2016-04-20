using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTurtles.ServiceTestRunnerLib.Utilities
{
    [Flags]
    public enum ErrorModes
    {
        Default = 0x0,
        FailCriticalErrors = 0x1,
        NoGpFaultErrorBox = 0x2, // &lt;- this is the one we need
        NoAlignmentFaultExcept = 0x4,
        NoOpenFileErrorBox = 0x8000,
    }

    public class ErrorModeContext : IDisposable
    {
        private readonly int _oldMode;

        public ErrorModeContext(ErrorModes mode)
        {
            _oldMode = SetErrorMode((int)mode);
        }

        ~ErrorModeContext()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            SetErrorMode(_oldMode);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [DllImport("kernel32.dll")]
        private static extern int SetErrorMode(int newMode);
    }
}
