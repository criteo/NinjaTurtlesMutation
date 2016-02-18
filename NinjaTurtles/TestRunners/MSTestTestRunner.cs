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
// License along with NinjaTurtles.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012-14 David Musgrove and others.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NinjaTurtles.TestRunners
{
    /// <summary>
    /// An implementation of <see cref="ITestRunner" /> to run a unit test
    /// suite using the MSTest console runner.
    /// </summary>
    /// <example>
    /// <para>
    /// This code creates and runs the default set of mutation tests for the
    /// <b>ClassUnderTest</b> class's <b>MethodUnderTest</b> method using 
    /// the <see cref="MSTestTestRunner" />:
    /// </para>
    /// <code lang="cs">
    /// MutationTestBuilder&lt;ClassUnderTest&gt;
    ///     .For("MethodUnderTest")
    ///     .UsingRunner&lt;MSTestTestRunner&gt;()
    ///     .Run();
    /// </code>
    /// <code lang="vbnet">
    /// Call MutationTestBuilder(Of ClassUnderTest) _
    ///     .For("MethodUnderTest") _
    ///     .UsingRunner(Of MSTestTestRunner)() _
    ///     .Run
    /// </code>
    /// <code lang="cpp">
    /// MutationTestBuilder&lt;ClassUnderTest^&gt;
    ///     ::For("MethodUnderTest")
    ///     ->UsingRunner&lt;MSTestTestRunner^&gt;()
    ///     ->Run();
    /// </code>
    /// </example>
// ReSharper disable InconsistentNaming
    public class MSTestTestRunner : TestRunnerBase
// ReSharper restore InconsistentNaming
    {
        /// <summary>
        /// Runs the tests specified from the test assembly, found within the
        /// test directory identified in the provided
        /// <see cref="MutantMetaData" /> instance.
        /// <remarks>
        /// This method won't be called
        /// from a user's testing code, it is called internally by
        /// NinjaTurtles, and is only exposed publicly to allow for a new
        /// implementation to be provided as an extension to NinjaTurtles.
        /// </remarks>
        /// </summary>
        /// <param name="testDirectory">
        /// The <see cref="TestDirectory" /> containing the test image.
        /// </param>
        /// <param name="testAssemblyLocation">
        ///   The file name (with or without path) of the unit test assembly.
        /// </param>
        /// <param name="testsToRun">
        ///   A list of qualified unit test names.
        /// </param>
        /// <returns>
        /// A <see cref="Process" /> instance to run the unit test runner.
        /// </returns>
        public override Process GetRunnerProcess(TestDirectory testDirectory, string testAssemblyLocation, IEnumerable<string> testsToRun)
        {
            testAssemblyLocation = Path.Combine(testDirectory.FullName, Path.GetFileName(testAssemblyLocation));
            string testArguments = string.Join(" ", testsToRun.Select(t => string.Format("/test:\"{0}\"", t)));
            string arguments = string.Format("/testcontainer:\"{0}\" {1}",
                                             testAssemblyLocation, testArguments);

            var searchPath = new List<string>();
            string programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86Folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            AddSearchPathsForVisualStudioVersions(searchPath, programFilesFolder);
            if (!string.IsNullOrEmpty(programFilesX86Folder))
            {
                AddSearchPathsForVisualStudioVersions(searchPath, programFilesX86Folder);
            }
            return ConsoleProcessFactory.CreateProcess("MSTest.exe", arguments, searchPath);
        }

        /// <summary>
        /// Ensures that the chosen runner can actually be found and executed.
        /// </summary>
        /// <param name="testDirectory"></param>
        /// <param name="testAssemblyLocation"></param>
        /// <remarks>
        /// This method won't be called
        /// from a user's testing code, it is called internally by
        /// NinjaTurtles, and is only exposed publicly to allow for a new
        /// implementation to be provided as an extension to NinjaTurtles.
        /// </remarks>
        public override void EnsureRunner(TestDirectory testDirectory, string testAssemblyLocation)
        {
            try
            {
                var searchPath = new List<string>();
                string programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string programFilesX86Folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                AddSearchPathsForVisualStudioVersions(searchPath, programFilesFolder);
                if (!string.IsNullOrEmpty(programFilesX86Folder))
                {
                    AddSearchPathsForVisualStudioVersions(searchPath, programFilesX86Folder);
                }

                var process = ConsoleProcessFactory.CreateProcess("MSTest.exe", string.Empty, searchPath);
                process.Start();
            }
            catch (Exception)
            {
                throw new TestRunnerException("Unable to find mstest.exe.");
            }
        }

        private void AddSearchPathsForVisualStudioVersions(ICollection<string> searchPath, string baseFolder)
        {
            for (int visualStudioVersion = 10; visualStudioVersion <= 13; visualStudioVersion++)
            {
                searchPath.Add(Path.Combine(baseFolder,
                    string.Format("Microsoft Visual Studio {0}.0\\Common7\\IDE", visualStudioVersion)));
            }
        }
    }
}
