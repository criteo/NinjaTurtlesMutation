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
using System.IO;
using System.Threading;

namespace NinjaTurtles
{
    /// <summary>
    /// Structure giving metadata for a mutation test.
    /// </summary>
    public sealed class MutationTestMetaData : IDisposable
    {
        /// <summary>
        /// Gets or sets a string describing the code difference obtained
        /// by decompiling the expanded IL of both the original and the 
        /// mutated methods.
        /// </summary>
        public string DiffRepresentation { get; set; }

        /// <summary>
        /// Gets or sets the target folder for the mutation test, to which
        /// the test DLLs and mutated assembly have been copied.
        /// </summary>
        public string TestFolder { get; set; }

        /// <summary>
        /// Gets or sets the description of the mutation test being run.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                int attemptCount = 0;
                do
                {
                    try
                    {
                        Directory.Delete(TestFolder, true);
                    }
                    catch
                    {
                    }
                    if (Directory.Exists(TestFolder)) Thread.Sleep(1000);
                } while (Directory.Exists(TestFolder) && attemptCount++ < 3);
            }
        }
    }
}
