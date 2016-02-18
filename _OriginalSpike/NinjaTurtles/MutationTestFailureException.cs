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

namespace NinjaTurtles
{
    /// <summary>
    /// A custom exception type that is thrown to indicate a mutation test
    /// failure. Test frameworks that do not simply fail their tests on an
    /// exception being thrown can catch this exception type and act
    /// accordingly.
    /// </summary>
    public sealed class MutationTestFailureException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="MutationTestFailureException" /> class with the default
        /// message.
        /// </summary>
        public MutationTestFailureException() : this("Not all mutations caused the suite to fail.") {}

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="MutationTestFailureException" /> class with the specified
        /// message.
        /// </summary>
        /// <param name="message">
        /// The message to use.
        /// </param>
        public MutationTestFailureException(string message) : base(message) { }
    }
}
