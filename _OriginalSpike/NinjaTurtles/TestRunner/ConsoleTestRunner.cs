#region Copyright & licence

// This file is part of NinjaTurtles.
// 
// NinjaTurtles is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// NinjaTurtles is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with Refix.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Mono.Cecil;

using NinjaTurtles.Attributes;

namespace NinjaTurtles.TestRunner
{
    /// <summary>
    /// An abstract implementation of <see cref="ITestRunner" /> that uses the
    /// <see cref="Process"/> class to run a console-based test runner.
    /// </summary>
    public abstract class ConsoleTestRunner : ITestRunner
    {
        /// <summary>
        /// Runs the test suite for the specified method, identifying the tests
        /// by inspecting the assembly identified by the
        /// <paramref name="testLibraryPath" /> for
        /// <see cref="MethodTestedAttribute"/>s.
        /// </summary>
        /// <param name="method">
        /// A <see cref="MethodDefinition" /> defining the method for which
        /// mutation tests should be run.
        /// </param>
        /// <param name="testLibraryPath">
        /// The path to an assembly containing unit tests for the method to be
        /// tested.
        /// </param>
        /// <param name="tests">
        /// A list of fully-qualified test names to test.
        /// </param>
        /// <returns>
        /// <b>true</b> if the tests pass (which is bad in the context of
        /// mutation testing, <b>false</b> if at least one fails, or
        /// <b>null</b> if no valid tests are found in the assembly.
        /// </returns>
        public bool? RunTestsWithMutations(MethodDefinition method, string testLibraryPath,
            IEnumerable<string> tests)
        {
            var startInfo = new ProcessStartInfo(GetCommandLineFileName())
                                {
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
									RedirectStandardOutput = true
                                };
            startInfo.Arguments = GetCommandLineArguments(testLibraryPath, tests);
			if (Runtime.IsRunningOnMono)
			{
				startInfo.Arguments = startInfo.FileName + " " + startInfo.Arguments;
				startInfo.FileName = "mono";
			}
            using (var process = Process.Start(startInfo))
            {
                if (!process.WaitForExit(30000))
                {
                    return null;
                }
                return InterpretExitCode(process.ExitCode);
            }
        }

        /// <summary>
        /// Gets the command line executable file name used to run the unit
        /// tests.
        /// </summary>
        /// <returns></returns>
        protected abstract string GetCommandLineFileName();

        /// <summary>
        /// Gets the arguments used to run the unit tests specified in the
        /// <paramref name="tests" /> parameter from the library found at path
        /// <paramref name="testLibraryPath" />.
        /// </summary>
        /// <param name="testLibraryPath">
        /// The path to the test assembly.
        /// </param>
        /// <param name="tests">
        /// A list of the fully qualified names of the test methods to be run.
        /// </param>
        /// <returns></returns>
        protected abstract string GetCommandLineArguments(string testLibraryPath, IEnumerable<string> tests);

        /// <summary>
        /// Maps a process exit code to the success status of the test suite.
        /// </summary>
        /// <param name="exitCode">
        /// The exit code of the console test runner process.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test suite passed, otherwise <b>false</b>.
        /// </returns>
        protected abstract bool InterpretExitCode(int exitCode);

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the 
        /// runtime from inside the finalizer and you should not reference 
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">
        /// Indicates whether the <see mref="Dispose" /> method is being
        /// called.
        /// </param>
        protected abstract void Dispose(bool disposing);
    }
}
