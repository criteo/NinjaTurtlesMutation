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

namespace NinjaTurtles
{
    /// <summary>
    /// A factory class used to instatiate a <see cref="Process" /> instance,
    /// taking into account the operating system and runtime.
    /// </summary>
    public static class ConsoleProcessFactory
    {
        internal static bool IsMono { get; set; }
        internal static bool IsWindows { get; set; }

        static ConsoleProcessFactory()
        {
            IsMono = Type.GetType("Mono.Runtime") != null;
            IsWindows = Environment.OSVersion.Platform.ToString().StartsWith("Win")
                        || Environment.OSVersion.Platform == PlatformID.Xbox;
        }

        /// <summary>
        /// Creates a <see cref="Process" /> instance used to execute the
        /// executable identifier by the <paramref name="exeName"/>
        /// parameter, with the <paramref name="arguments"/> specified.
        /// </summary>
        /// <param name="exeName">
        /// The name (and path) of the executable to run.
        /// </param>
        /// <param name="arguments">
        /// The command line arguments to pass to the executable.
        /// </param>
        /// <param name="additionalSearchLocations">
        /// An optional list of additional search paths.
        /// </param>
        /// <returns>
        /// An instance of <see cref="Process" />.
        /// </returns>
        public static Process CreateProcess(
            string exeName,
            string arguments,
            IEnumerable<string> additionalSearchLocations = null)
        {
            exeName = FindExecutable(exeName, additionalSearchLocations);

            if (IsMono)
            {
                arguments = string.Format("--runtime=v4.0 \"{0}\" {1}", exeName, arguments);
                exeName = "mono";
            }

            arguments = string.Format(arguments, IsWindows ? "/" : "-");

            return new Process
            {
                StartInfo = new ProcessStartInfo(exeName, arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                }
            };
        }

        private static string FindExecutable(string exeName, IEnumerable<string> additionalSearchLocations)
        {
            var searchPath = new List<string>();
            if (additionalSearchLocations != null) searchPath.AddRange(additionalSearchLocations);
            string environmentSearchPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            searchPath.AddRange(environmentSearchPath.Split(IsWindows ? ';' : ':'));

            foreach (string folder in searchPath)
            {
                var fullExePath = Path.Combine(folder, exeName);
                if (File.Exists(fullExePath))
                {
                    return fullExePath;
                }
            }
            return exeName;
        }
    }
}
