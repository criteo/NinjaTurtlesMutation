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
using NLog;

namespace NinjaTurtles.TestRunners
{
    /// <summary>
    /// An implementation of <see cref="ITestRunner" /> to run a unit test
    /// suite using the NUnit console runner.
    /// </summary>
    /// <example>
    /// <para>
    /// This code creates and runs the default set of mutation tests for the
    /// <b>ClassUnderTest</b> class's <b>MethodUnderTest</b> method using 
    /// the <see cref="NUnitTestRunner" />:
    /// </para>
    /// <code lang="cs">
    /// MutationTestBuilder&lt;ClassUnderTest&gt;
    ///     .For("MethodUnderTest")
    ///     .UsingRunner&lt;NUnitTestRunner&gt;()
    ///     .Run();
    /// </code>
    /// <code lang="vbnet">
    /// Call MutationTestBuilder(Of ClassUnderTest) _
    ///     .For("MethodUnderTest") _
    ///     .UsingRunner(Of NUnitTestRunner)() _
    ///     .Run
    /// </code>
    /// <code lang="cpp">
    /// MutationTestBuilder&lt;ClassUnderTest^&gt;
    ///     ::For("MethodUnderTest")
    ///     ->UsingRunner&lt;NUnitTestRunner^&gt;()
    ///     ->Run();
    /// </code>
    /// <para>
    /// Alternatively, this can be omitted, since this is the default runner
    /// used by NinjaTurtles.
    /// </para>
    /// </example>
    public class NUnitTestRunner : TestRunnerBase
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
            string originalTestAssemblyLocation = testAssemblyLocation;
            testAssemblyLocation = Path.Combine(testDirectory.FullName, Path.GetFileName(testAssemblyLocation));
            string testListFile = Path.Combine(testDirectory.FullName, "ninjaturtlestestlist.txt");

            File.WriteAllLines(testListFile, testsToRun);
            string arguments = string.Format("\"{0}\" {{0}}runlist=\"{1}\" {{0}}noshadow {{0}}nologo {{0}}nodots {{0}}stoponerror", testAssemblyLocation, testListFile);

            var searchPath = new List<string>();

            AddSearchPathTermsForNUnitVersion(testAssemblyLocation, originalTestAssemblyLocation, searchPath);

            return ConsoleProcessFactory.CreateProcess("nunit-console.exe", arguments, searchPath);
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
                string originalTestAssemblyLocation = testAssemblyLocation;
                testAssemblyLocation = Path.Combine(testDirectory.FullName, Path.GetFileName(testAssemblyLocation));
                
                var searchPath = new List<string>();
                AddSearchPathTermsForNUnitVersion(testAssemblyLocation, originalTestAssemblyLocation, searchPath);

                string arguments = string.Format("/?");
                var process = ConsoleProcessFactory.CreateProcess("nunit-console.exe", arguments, searchPath);
                process.Start();
            }
            catch (Exception)
            {
                throw new TestRunnerException("Unable to find nunit-console.exe.\n  Either install the version of nunit you are using from the MSI, or \n use nuget to install nunit.runners in the packages directory.");
            }
        }

        private static void AddSearchPathTermsForNUnitVersion(string testAssemblyLocation, string originalTestAssemblyLocation, ICollection<string> searchPath)
        {
            var nUnitReference =
                new Module(testAssemblyLocation).AssemblyDefinition.MainModule.AssemblyReferences.FirstOrDefault(
                    r => r.Name == "nunit.framework");

            if (nUnitReference == null) return;

            string programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86Folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string bestGuessSolutionFolderWithNuGet = GetBestGuessSolutionFolder(originalTestAssemblyLocation);

            Version version = nUnitReference.Version;

            if (bestGuessSolutionFolderWithNuGet != null)
            {
                AddSearchPathsForVersionVariants(searchPath, version,
                    bestGuessSolutionFolderWithNuGet, "packages\\NUnit.Runners.{0}\\tools");
            }
            AddSearchPathsForVersionVariants(searchPath, version,
                programFilesFolder, "NUnit {0}\\bin");
            if (!string.IsNullOrEmpty(programFilesX86Folder))
            {
                AddSearchPathsForVersionVariants(searchPath, version,
                    programFilesX86Folder, "NUnit {0}\\bin");
            }
        }
    }
}
