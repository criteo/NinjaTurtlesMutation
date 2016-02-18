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
    /// suite using the Gallio console runner.
    /// </summary>
    /// <example>
    /// <para>
    /// This code creates and runs the default set of mutation tests for the
    /// <b>ClassUnderTest</b> class's <b>MethodUnderTest</b> method using 
    /// the <see cref="GallioTestRunner" />:
    /// </para>
    /// <code lang="cs">
    /// MutationTestBuilder&lt;ClassUnderTest&gt;
    ///     .For("MethodUnderTest")
    ///     .UsingRunner&lt;GallioTestRunner&gt;()
    ///     .Run();
    /// </code>
    /// <code lang="vbnet">
    /// Call MutationTestBuilder(Of ClassUnderTest) _
    ///     .For("MethodUnderTest") _
    ///     .UsingRunner(Of GallioTestRunner)() _
    ///     .Run
    /// </code>
    /// <code lang="cpp">
    /// MutationTestBuilder&lt;ClassUnderTest^&gt;
    ///     ::For("MethodUnderTest")
    ///     ->UsingRunner&lt;GallioTestRunner^&gt;()
    ///     ->Run();
    /// </code>
    /// </example>
    public class GallioTestRunner : TestRunnerBase
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
            IDictionary<string, IList<string>> testsByFixture = new Dictionary<string, IList<string>>();
            foreach (var test in testsToRun)
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

            string originalTestAssemblyLocation = testAssemblyLocation;
            testAssemblyLocation = Path.Combine(testDirectory.FullName, Path.GetFileName(testAssemblyLocation));
            string arguments = string.Format("\"{0}\" {{0}}f:\"{1}\" {{0}}r:IsolatedProcess {{0}}sr",
                                 testAssemblyLocation, filter);

            var searchPath = new List<string>();

            string solutionFolder = GetBestGuessSolutionFolder(originalTestAssemblyLocation);
            if (!string.IsNullOrEmpty(solutionFolder))
            {
                DirectoryInfo gallioFolder = new DirectoryInfo(solutionFolder)
                    .GetDirectories("packages").Single()
                    .GetDirectories( "GallioBundle*").FirstOrDefault();
                if (gallioFolder != null)
                {
                    searchPath.Add(Path.Combine(gallioFolder.FullName, "bin"));
                }
            }

            string programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            searchPath.Add(Path.Combine(programFilesFolder, "Gallio\\bin"));
            string programFilesX86Folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (!string.IsNullOrEmpty(programFilesX86Folder))
            {
                searchPath.Add(Path.Combine(programFilesX86Folder, "Gallio\\bin"));
            }
            return ConsoleProcessFactory.CreateProcess("Gallio.Echo.exe", arguments, searchPath);
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

                string originalTestAssemblyLocation = testAssemblyLocation;
                string solutionFolder = GetBestGuessSolutionFolder(originalTestAssemblyLocation);
                if (!string.IsNullOrEmpty(solutionFolder))
                {
                    DirectoryInfo gallioFolder = new DirectoryInfo(solutionFolder)
                        .GetDirectories("packages").Single()
                        .GetDirectories("GallioBundle*").FirstOrDefault();
                    if (gallioFolder != null)
                    {
                        searchPath.Add(Path.Combine(gallioFolder.FullName, "bin"));
                    }
                }

                string programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                searchPath.Add(Path.Combine(programFilesFolder, "Gallio\\bin"));
                string programFilesX86Folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                if (!string.IsNullOrEmpty(programFilesX86Folder))
                {
                    searchPath.Add(Path.Combine(programFilesX86Folder, "Gallio\\bin"));
                }
                var process = ConsoleProcessFactory.CreateProcess("Gallio.Echo.exe", string.Empty, searchPath);
                process.Start();
            }
            catch (Exception)
            {
                throw new TestRunnerException("Unable to find Gallio.Echo.exe.\n  Either install the version of gallio you are using, or \n use nuget to install GallioBundle in the packages directory.");
            }
        }
    }
}
