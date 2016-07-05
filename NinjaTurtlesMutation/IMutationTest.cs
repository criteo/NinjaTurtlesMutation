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
using NinjaTurtlesMutation.Turtles;

namespace NinjaTurtlesMutation
{
    /// <summary>
    /// An interface forming the core of a fluent interface for defining
    /// and running mutation tests using NinjaTurtles.
    /// </summary>
    /// <remarks>
    /// Client code will not use this interface explicitly, nor attempt to
    /// instantiate an implementing class. Instead, this interface is exposed
    /// by the <see cref="MutationTestBuilder" /> class's 
    /// <see mref="MutationTestBuilder{T}.For" /> method, which forms the start
    /// of a chain of fluent statements defining a set of mutation tests.
    /// </remarks>
    public interface IMutationTest
	{
        /// <summary>
        /// Gets the type which is the target of the current mutation test.
        /// </summary>
		Type TargetType { get; }

        /// <summary>
        /// Gets the name of the method which is the target of the current
        /// mutation test.
        /// </summary>
		string TargetMethod { get; }

        /// <summary>
        /// Runs the defined mutation tests and return the mutation score.
        /// </summary>
        float Run(bool detachBench);

        /// <summary>
        /// Adds a <see cref="IMethodTurtle" /> type to be used in mutation
        /// testing. If no turtles are added, then all turtles from the
        /// NinjaTurtles assembly are added by default.
        /// </summary>
        /// <typeparam name="T">
        /// A type that implements <see cref="IMethodTurtle" />.
        /// </typeparam>
        /// <returns>
        /// The original <see cref="IMutationTest" /> instance to allow fluent
        /// method chaining.
        /// </returns>
        IMutationTest With<T>() where T : IMethodTurtle;

        /// <summary>
        /// Specify which types of turtles to use during mutation test.
        /// </summary>
        /// <param name="turtleSet">
        /// A set of IMethodTurtle types
        /// </param>
        void With(ISet<Type> turtleSet);

        /// <summary>
        /// Specifies a file name to which mutation test results should be
        /// written. If the file already exists, it will be overwritten.
        /// </summary>
        /// <param name="fileName">
        /// The path and file name of the output file.
        /// </param>
        /// <returns>
        /// The original <see cref="IMutationTest" /> instance to allow fluent
        /// method chaining.
        /// </returns>
        IMutationTest WriteReportTo(string fileName);

        /// <summary>
        /// Specifies a file name to which mutation test results should be
        /// written. If the file already exists, then the results of the
        /// current mutation test will be merged into it.
        /// </summary>
        /// <param name="fileName">
        /// The path and file name of the output file.
        /// </param>
        /// <returns>
        /// The original <see cref="IMutationTest" /> instance to allow fluent
        /// method chaining.
        /// </returns>
        IMutationTest MergeReportTo(string fileName);
	}
}

