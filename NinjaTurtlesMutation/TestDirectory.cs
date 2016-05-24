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
using System.IO;
using System.Runtime.InteropServices;
using NLog;

namespace NinjaTurtlesMutation
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

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CreateSymbolicLink(
        string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDirectory" />
        /// class.
        /// </summary>
        public TestDirectory()
		{
            _folder = Path.Combine(Path.GetTempPath(),
                                   "NinjaTurtlesMutation",
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
        /// Initializes a new instance of the <see cref="TestDirectory" />
        /// class. Instead of copying file, this constructor will only
        /// create symbolic links except for the files specified in the
        /// toCopy list
        /// </summary>
        /// <param name="sourceFolder">
        /// The name of a folder whose contents should be recursively
        /// copied to the temporary folder.
        /// </param>
        /// <param name="toCopy">
        /// An IList containing the names of files to copy.
        /// </param>
        public TestDirectory(string sourceFolder, IList<string> toCopy)
            : this()
        {
            _log.Debug("Copying contents from folder \"{0}\".", sourceFolder);
            SymlinkDirectoryContents(sourceFolder, _folder, toCopy);
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

        private static void SymlinkDirectoryContents
            (string directory, string targetDirectory, IList<string> toCopy)
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                string fileName = Path.GetFileName(file);
                _log.Trace("Copying file \"{0}\".", fileName);
                // ReSharper disable once AssignNullToNotNullAttribute
                string target = Path.Combine(targetDirectory, fileName);
                if (toCopy.Contains(fileName))
                    File.Copy(file, target);
                else
                    CreateSymbolicLink(target, file, SymbolicLink.File);
            }
            foreach (var subDirectory in Directory.GetDirectories(directory))
            {
                string subDirectoryName = Path.GetFileName(subDirectory);
                _log.Trace("Creating subdirectory \"{0}\".", subDirectoryName);
                // ReSharper disable once AssignNullToNotNullAttribute
                string target = Path.Combine(targetDirectory, subDirectoryName);
                Directory.CreateDirectory(target);
                SymlinkDirectoryContents(subDirectory, target, toCopy);
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
                _log.Error(ex, message);
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

