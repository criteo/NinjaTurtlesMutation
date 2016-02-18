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

using NinjaTurtles.TestRunner;

namespace NinjaTurtles
{
    /// <summary>
    /// Exposes static properties related to the runtime platform on which
    /// NinjaTurtles is operating. This allows specific implementations of
    /// <see cref="ITestRunner" /> to cater for, for example, running on
    /// Mono rather than the CLR, or on a non-Windows operating system.
    /// </summary>
    /// <remarks>
    /// This class is exposed publicly to assist users in implementing
    /// their own implementation of <see cref="ITestRunner" />, or concrete
    /// implementations of <see cref="ConsoleTestRunner" />.
    /// </remarks>
	public static class Runtime
	{
		private static readonly bool _isMono;
	    private static readonly bool _isWindows;
		
		static Runtime()
		{
			_isMono = Type.GetType("Mono.Runtime") != null;
		    _isWindows = Environment.OSVersion.Platform.ToString().StartsWith("Win")
		                 || Environment.OSVersion.Platform == PlatformID.Xbox;
		}

        /// <summary>
        /// Gets a <see cref="bool" /> value indicating whether NinjaTurtles is
        /// currently running on a Mono platform.
        /// </summary>
        public static bool IsRunningOnMono
		{
    		get { return _isMono; }
		}

        /// <summary>
        /// Gets a <see cref="bool" /> value indicating whether NinjaTurtles is
        /// currently running on a Windows platform.
        /// </summary>
        public static bool IsRunningOnWindows
	    {
            get { return _isWindows; }
	    }

        /// <summary>
        /// Gets the seperator used in the environment's search path to
        /// separate individual operating system paths. On a Windows system,
        /// this is a semi-colon; on a Unix-like system, it is a colon.
        /// </summary>
        /// <example>
        /// The following example demonstrates the use of this property in
        /// the <see cref="NUnitTestRunner" /> class.
        /// <code lang="cs">
        /// searchPath.AddRange(environmentSearchPath
        ///	    .Split(Runtime.SearchPathSeparator));
        /// </code>
        /// <code lang="vbnet">
        /// searchPath.AddRange(environmentSearchPath
        ///	    .Split(Runtime.SearchPathSeparator))
        /// </code>
        /// <code lang="cpp">
        /// searchPath.AddRange(environmentSearchPath
        ///	    ->Split(NinjaTurtles::Runtime::SearchPathSeparator));
        /// </code>
        /// </example>
        public static char SearchPathSeparator
		{
            get { return _isWindows ? ';' : ':'; }
		}

        /// <summary>
        /// Gets the prefix used for a command line argument with a utility
        /// that has chosen to use a forward slash on a Windows system (such
        /// as the NUnit console runner).
        /// </summary>
        /// <remarks>
        /// Typically, the forward slash (/) will be replaced with a dash (-)
        /// on a Unix-like system, and that is what is represented by this
        /// property. Utilities with another convention should implement their
        /// own logic based on the <see pref="IsRunningOnWindows" />
        /// property.
        /// </remarks>
        public static string CommandLineArgumentCharacter
		{
            get { return _isWindows ? "/" : "-"; }
		}
	}
}

