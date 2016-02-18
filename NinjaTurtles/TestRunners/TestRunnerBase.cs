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
    /// A base class for implementations of <see cref="ITestRunner"/> which
    /// exposes a helper method to guess the root solution folder based on the
    /// presence of both a packages folder (NuGet) and a .sln file.
    /// </summary>
    public abstract class TestRunnerBase : ITestRunner
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
        public abstract Process GetRunnerProcess(TestDirectory testDirectory, string testAssemblyLocation, IEnumerable<string> testsToRun);

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
        public abstract void EnsureRunner(TestDirectory testDirectory, string testAssemblyLocation);

        /// <summary>
        /// Traverses the directory tree from the location of the original test
        /// assembly, looking for both a packages folder (NuGet) and a .sln
        /// file.
        /// </summary>
        /// <param name="originalTestAssemblyDirectory">
        ///  The folder containing the assembly under test.
        /// </param>
        /// <returns>
        ///  A folder believed to be the root folder of the solution, or
        /// <b>null</b> (<b>Nothing</b> in Visual Basic) if not found.
        /// </returns>
        protected static string GetBestGuessSolutionFolder(string originalTestAssemblyDirectory)
        {
            string bestGuessSolutionFolderWithNuGet = null;
            var directory = new DirectoryInfo(originalTestAssemblyDirectory);
            while ((directory = directory.Parent) != null)
            {
                if (directory.GetDirectories("packages").Any()
                    && directory.GetFiles("*.sln").Any())
                {
                    bestGuessSolutionFolderWithNuGet = directory.FullName;
                    break;
                }
            }
            return bestGuessSolutionFolderWithNuGet;
        }

        /// <summary>
        /// Adds folders to the search path used to try to locate a test
        /// runner, including the specified version in two-, three- and
        /// four-part variants.
        /// </summary>
        /// <param name="searchPath">
        /// The current collection of search folders.
        /// </param>
        /// <param name="version">
        /// The version of the test framework in use.
        /// </param>
        /// <param name="baseFolder">
        /// The base (absolute) folder location for the search, normally the
        /// solution's root folder.
        /// </param>
        /// <param name="pathFormat">
        /// The relative folder under the <paramref name="baseFolder"/> into
        /// which the version number will be injected. This should contain a
        /// single parameter placeholder.
        /// </param>
        protected static void AddSearchPathsForVersionVariants(ICollection<string> searchPath, Version version,
            string baseFolder, string pathFormat)
        {
            var fourPart = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build,
                version.Revision);
            var threePart = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            var twoPart = string.Format("{0}.{1}", version.Major, version.Minor);

            searchPath.Add(Path.Combine(baseFolder, string.Format(pathFormat, fourPart)));
            searchPath.Add(Path.Combine(baseFolder, string.Format(pathFormat, threePart)));
            searchPath.Add(Path.Combine(baseFolder, string.Format(pathFormat, twoPart)));
        }
    }
}
