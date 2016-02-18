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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NinjaTurtles.TestRunner
{
    /// <summary>
    /// A concrete implementation of <see cref="ConsoleTestRunner" /> that
    /// attempts to locate and run the Gallio.Echo console runner.
    /// </summary>
    public sealed class GallioTestRunner : ConsoleTestRunner
    {
        private const string EXECUTABLE_NAME = "Gallio.Echo.exe";

        private string _runnerPath;

        private string RunnerPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_runnerPath))
                {
                    _runnerPath = FindConsoleRunner();
                }
                return _runnerPath;
            }
            set { _runnerPath = value; }
        }

        private string FindConsoleRunner()
        {
            var searchPath = new List<string>();

            string programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            searchPath.AddRange(new[]
                                    {
                                        Path.Combine(programFilesFolder, "Gallio\\bin")
                                    });
			string programFilesX86Folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (!string.IsNullOrEmpty(programFilesX86Folder))
			{
	            searchPath.AddRange(new[]
                        {
                                        Path.Combine(programFilesFolder, "Gallio\\bin")
                        });
			}
			string environmentSearchPath = Environment.GetEnvironmentVariable("PATH") ?? "";
            searchPath.AddRange(environmentSearchPath
			    .Split(new[] {Runtime.SearchPathSeparator}, StringSplitOptions.RemoveEmptyEntries));
            foreach (string folder in searchPath)
            {
                if (File.Exists(Path.Combine(folder, EXECUTABLE_NAME)))
                {
                    return RunnerPath = folder;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the command line executable file name used to run the unit
        /// tests.
        /// </summary>
        /// <returns></returns>
        protected override string GetCommandLineFileName()
        {
            return Path.Combine(RunnerPath, EXECUTABLE_NAME);
        }

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
        protected override string GetCommandLineArguments(string testLibraryPath, IEnumerable<string> tests)
        {
            IDictionary<string, IList<string>> testsByFixture = new Dictionary<string, IList<string>>();
            foreach (var test in tests)
            {
                int position = test.LastIndexOf(".");
                string fixture = test.Substring(0, position);
                string member = test.Substring(position + 1);
                if (!testsByFixture.ContainsKey(fixture))
                {
                    testsByFixture.Add(fixture, new List<string>());
                }
                testsByFixture[fixture].Add(member);
            }
            string filter = string.Join(" or ", testsByFixture.Select(
                kv => string.Format("(Type: {0} and Member: {1})",
                                    kv.Key,
                                    string.Join(", ", kv.Value))));
            string exeCommand = string.Format("\"{1}\" {0}f:\"{2}\" /r:IsolatedProcess",
                                 Runtime.CommandLineArgumentCharacter,
                                 testLibraryPath, filter);
            return exeCommand;
        }

        /// <summary>
        /// Maps a process exit code to the success status of the test suite.
        /// </summary>
        /// <param name="exitCode">
        /// The exit code of the console test runner process.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test suite passed, otherwise <b>false</b>.
        /// </returns>
        protected override bool InterpretExitCode(int exitCode)
        {
            return exitCode == 0;
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
        protected override void Dispose(bool disposing)
        {
        }
    }
}