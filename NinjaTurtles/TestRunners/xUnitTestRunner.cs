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

using Mono.Cecil;

namespace NinjaTurtles.TestRunners
{
    /// <summary>
    /// An implementation of <see cref="ITestRunner" /> to run a unit test
    /// suite using the xUnit console runner.
    /// </summary>
    /// <example>
    /// <para>
    /// This code creates and runs the default set of mutation tests for the
    /// <b>ClassUnderTest</b> class's <b>MethodUnderTest</b> method using 
    /// the <see cref="xUnitTestRunner" />:
    /// </para>
    /// <code lang="cs">
    /// MutationTestBuilder&lt;ClassUnderTest&gt;
    ///     .For("MethodUnderTest")
    ///     .UsingRunner&lt;xUnitTestRunner&gt;()
    ///     .Run();
    /// </code>
    /// <code lang="vbnet">
    /// Call MutationTestBuilder(Of ClassUnderTest) _
    ///     .For("MethodUnderTest") _
    ///     .UsingRunner(Of xUnitTestRunner)() _
    ///     .Run
    /// </code>
    /// <code lang="cpp">
    /// MutationTestBuilder&lt;ClassUnderTest^&gt;
    ///     ::For("MethodUnderTest")
    ///     ->UsingRunner&lt;xUnitTestRunner^&gt;()
    ///     ->Run();
    /// </code>
    /// </example>
// ReSharper disable InconsistentNaming
    public class xUnitTestRunner : TestRunnerBase
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
            string originalTestAssemblyLocation = testAssemblyLocation;
            testAssemblyLocation = Path.Combine(testDirectory.FullName, Path.GetFileName(testAssemblyLocation));

            // HACKTAG: In the absence of a simple way to limit the tests
            // xUnit runs, we inject TraitAttributes to specify this.
            var testModule = new Module(testAssemblyLocation);
            var xUnitModule =
                AssemblyDefinition.ReadAssembly(Path.Combine(Path.GetDirectoryName(testAssemblyLocation), "xunit.dll"));
            var traitConstructor =
                testModule.Definition.Import(
                    xUnitModule.MainModule
                        .Types.Single(t => t.Name == "TraitAttribute")
                        .Methods.Single(m => m.Name == Methods.CONSTRUCTOR));
            var customAttribute = new CustomAttribute(traitConstructor);
            customAttribute.ConstructorArguments.Add(
                new CustomAttributeArgument(
                    testModule.Definition.TypeSystem.String, "NinjaTurtles"));
            customAttribute.ConstructorArguments.Add(
                new CustomAttributeArgument(
                    testModule.Definition.TypeSystem.String, "run"));

            foreach (var module in testModule.Definition.Types)
            {
                foreach (var method in module.Methods)
                {
                    string qualifiedMethodName = module.FullName + "." + method.Name;
                    if (testsToRun.Contains(qualifiedMethodName))
                    {
                        method.CustomAttributes.Add(customAttribute);
                    }
                }
            }
            testDirectory.SaveAssembly(testModule);

            string arguments = string.Format("\"{0}\" {{0}}noshadow {{0}}trait \"NinjaTurtles=run\"",
                     testAssemblyLocation);

            var searchPath = new List<string>();

            AddSearchPathTermsForxUnitVersion(testAssemblyLocation, originalTestAssemblyLocation, searchPath);

            return ConsoleProcessFactory.CreateProcess("xunit.console.clr4.exe", arguments, searchPath);
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
                AddSearchPathTermsForxUnitVersion(testAssemblyLocation, originalTestAssemblyLocation, searchPath);

                var process = ConsoleProcessFactory.CreateProcess("xunit.console.clr4.exe", string.Empty, searchPath);
                process.Start();
            }
            catch (Exception)
            {
                throw new TestRunnerException("Unable to find xunit.console.clr4.exe.\n  Either install the version of xunit you are using from the MSI, or \n use nuget to install xunit.runners in the packages directory.");
            }
        }

        private static void AddSearchPathTermsForxUnitVersion(string testAssemblyLocation, string originalTestAssemblyLocation, ICollection<string> searchPath)
        {
            var xUnitReference =
                new Module(testAssemblyLocation).AssemblyDefinition.MainModule.AssemblyReferences.FirstOrDefault(
                    r => r.Name == "xunit");

            if (xUnitReference == null) return;

            string bestGuessSolutionFolderWithNuGet = GetBestGuessSolutionFolder(originalTestAssemblyLocation);

            Version version = xUnitReference.Version;

            if (bestGuessSolutionFolderWithNuGet != null)
            {
                AddSearchPathsForVersionVariants(searchPath, version,
                    bestGuessSolutionFolderWithNuGet, "packages\\xunit.runners.{0}\\tools");
            }
        }
    }
}
