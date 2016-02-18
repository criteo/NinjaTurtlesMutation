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
using System.IO;

using NLog;

namespace NinjaTurtles
{
    /// <summary>
    /// Represents a temporary directory used to contain a mutated assembly
    /// to be tested. The directory cleans up after itself when its
    /// <see mref="Dispose" /> method is called, unless its
    /// <see pref="DoNotDelete" /> property is set to <b>true</b>.
    /// </summary>
    public class TestDirectory : IDisposable
    {
        #region Logging

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #endregion

        private readonly string _folder;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDirectory" />
        /// class.
        /// </summary>
        public TestDirectory()
		{
            _folder = Path.Combine(Path.GetTempPath(),
                                   "NinjaTurtles",
                                   Guid.NewGuid().ToString("N"));
            _log.Debug("Creating folder \"{0}\".", _folder);
            Directory.CreateDirectory(_folder);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDirectory" />
        /// class.
        /// </summary>
        /// <param name="sourceFolder">
        /// The name of a folder whose contents should be recursively
        /// copied to the temporary folder.
        /// </param>
        public TestDirectory(string sourceFolder)
            : this()
		{
            _log.Debug("Copying contents from folder \"{0}\".", sourceFolder);
            CopyDirectoryContents(sourceFolder, _folder);
		}
		
        /// <summary>
        /// Saves an image of a mutated assembly into the root of the test
        /// directory.
        /// </summary>
        /// <param name="module"></param>
		public void SaveAssembly(Module module)
		{
		    string fileName = Path.GetFileName(module.AssemblyLocation);
		    string path = Path.Combine(_folder, fileName);
            _log.Debug("Writing assembly \"{0}\" to \"{1}\".", fileName, _folder);
            module.AssemblyDefinition.Write(path);
		}

	    private static void CopyDirectoryContents
            (string directory, string targetDirectory)
		{
			foreach (var file in Directory.GetFiles(directory))
			{
			    string fileName = Path.GetFileName(file);
                _log.Trace("Copying file \"{0}\".", fileName);
// ReSharper disable once AssignNullToNotNullAttribute
                string target = Path.Combine(targetDirectory, fileName);
                File.Copy(file, target);
			}
			foreach (var subDirectory in Directory.GetDirectories(directory))
			{
			    string subDirectoryName = Path.GetFileName(subDirectory);
                _log.Trace("Creating subdirectory \"{0}\".", subDirectoryName);
// ReSharper disable once AssignNullToNotNullAttribute
                string target = Path.Combine(targetDirectory, subDirectoryName);
                Directory.CreateDirectory(target);
				CopyDirectoryContents(subDirectory, target);
			}
		}
		
		/// <summary>
		/// Gets the full path of the test directory.
		/// </summary>
		public string FullName
		{
			get { return _folder; }
		}

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
		{
            if (DoNotDelete)
            {
                return;
            }
            try
            {
                _log.Debug("Deleting folder \"{0}\".", _folder);
                Directory.Delete(_folder, true);
            }
            catch (Exception ex)
            {
                string message = string.Format("Failed to delete folder \"{0}\".", _folder);
                _log.ErrorException(message, ex);
            }
		}

        /// <summary>
        /// Gets or sets a flag indicating whether or not the contents of the
        /// test directory should be allowed to remain on disk when the
        /// instance is disposed.
        /// </summary>
	    public bool DoNotDelete { get; set; }
    }
}

