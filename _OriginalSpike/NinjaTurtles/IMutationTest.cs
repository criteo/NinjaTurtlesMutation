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

using NinjaTurtles.TestRunner;
using NinjaTurtles.Turtles;
using NinjaTurtles.Turtles.Method;

namespace NinjaTurtles
{
    /// <summary>
    /// An interface forming the core of a fluent interface for defining
    /// and running mutation tests using NinjaTurtles.
    /// </summary>
    /// <remarks>
    /// Client code will not use this interface explicitly, nor attempt to
    /// instantiate an implementing class. Instead, this interface is exposed
    /// by the <see cref="MutationTestBuilder{T}" /> class's 
    /// <see mref="MutationTestBuilder{T}.For" /> method, which forms the start
    /// of a chain of fluent statements defining a set of mutation tests.
    /// </remarks>
    public interface IMutationTest
    {
        /// <summary>
        /// Runs the defined mutation tests using the specified test runner.
        /// </summary>
        /// <param name="maxThreads">
        /// Optionally, a maximum number of threads to spawn in testing. This
        /// can be usefully set to 1 when testing sequential mutators like the
        /// <see cref="OpCodeDeletionTurtle" />.
        /// </param>
        void Run(int? maxThreads = null);

        /// <summary>
        /// Adds a <see cref="ITurtle" /> type to be used in mutation testing.
        /// If no turtles are added, then all turtles from the NinjaTurtles
        /// assembly are added by default.
        /// </summary>
        /// <typeparam name="T">
        /// A type that implements <see cref="ITurtle" />.
        /// </typeparam>
        /// <returns>
        /// The original <see cref="IMutationTest" /> instance to allow fluent
        /// method chaining.
        /// </returns>
        IMutationTest With<T>() where T : ITurtle;

        /// <summary>
        /// Sets the unit test runner to be used, which is an implementation of
        /// <see cref="ITestRunner" />. If none is specified, then the default
        /// is to use <see cref="NUnitTestRunner" />.
        /// </summary>
        /// <typeparam name="T">
        /// A type that implements <see cref="ITestRunner" />.
        /// </typeparam>
        /// <returns>
        /// The original <see cref="IMutationTest" /> instance to allow fluent
        /// method chaining.
        /// </returns>
        IMutationTest UsingRunner<T>() where T : ITestRunner;
    }
}
