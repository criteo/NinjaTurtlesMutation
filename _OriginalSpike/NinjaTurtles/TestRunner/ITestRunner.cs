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

using Mono.Cecil;

using NinjaTurtles.Attributes;

namespace NinjaTurtles.TestRunner
{
    /// <summary>
    /// An <b>interface</b> that defines the contract a test runner must
    /// implement to be used by NinjaTurtles in running mutation tests.
    /// </summary>
    public interface ITestRunner : IDisposable
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
        /// The path to a library containing unit tests for the method to be
        /// tested.
        /// </param>
        /// <param name="tests">
        /// A list of fully-qualified test names to test.
        /// </param>
        /// <returns>
        /// <b>true</b> if the tests pass (which is bad in the context of
        /// mutation testing, <b>false</b> if at least one fails, or
        /// <b>null</b> if no valid tests are found in the library.
        /// </returns>
        bool? RunTestsWithMutations(MethodDefinition method, string testLibraryPath,
            IEnumerable<string> tests);
    }
}
