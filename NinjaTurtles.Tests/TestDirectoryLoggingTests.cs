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

using NUnit.Framework;

using NinjaTurtles.Tests.TestUtilities;
using NinjaTurtlesMutation;

namespace NinjaTurtles.Tests
{
	[TestFixture]
	public class TestDirectoryLoggingTests : LoggingTestFixture
	{
        [Test]
        public void Constructor_Logs_Created_Directory()
        {
            string path;
            using (var testDirectory = new TestDirectory())
            {
                path = testDirectory.FullName;
            }
            string expectedMessage = string.Format("DEBUG|Creating folder \"{0}\".|", path);
            AssertLogContains(expectedMessage);
        }
        
        [Test]
        public void Constructor_With_Copy_Logs_Source_Copy()
        {
            string tempFolder = Path.GetTempPath();
            string sourceFolder = Path.Combine(tempFolder, Guid.NewGuid().ToString("N"));
            string subFolder = Path.Combine(sourceFolder, "Ninja");
            string fileName = string.Format("{0:N}.txt", Guid.NewGuid());
            Directory.CreateDirectory(subFolder);
            File.WriteAllText(Path.Combine(subFolder, fileName), "Ninja");

            using (new TestDirectory(sourceFolder))
            {
            }
            string expectedMessage = string.Format("DEBUG|Copying contents from folder \"{0}\".|", sourceFolder);
            AssertLogContains(expectedMessage);
            expectedMessage = string.Format("TRACE|Creating subdirectory \"{0}\".|", "Ninja");
            AssertLogContains(expectedMessage);
            expectedMessage = string.Format("TRACE|Copying file \"{0}\".|", fileName);
            AssertLogContains(expectedMessage);
        }

        [Test]
        public void Dispose_Logs_Removal_Of_Directory()
        {
            string path;
            using (var testDirectory = new TestDirectory())
            {
                path = testDirectory.FullName;
            }
            string expectedMessage = string.Format("DEBUG|Deleting folder \"{0}\".|", path);
            AssertLogContains(expectedMessage);
        }

        [Test]
        public void Dispose_Logs_Exception_When_Folder_Is_Locked()
        {
            string path;
            FileStream fileStream;
            using (var testDirectory = new TestDirectory())
            {
                path = testDirectory.FullName;
                string fileName = Path.Combine(path, "locked.txt");
                fileStream = File.OpenWrite(fileName);
            }

            fileStream.Dispose();
            Directory.Delete(path, true);

            string expectedMessage = string.Format("ERROR|Failed to delete folder \"{0}\".|System.IO.IOException", path);
            AssertLogContains(expectedMessage, true);
        }

        [Test]
        public void SaveAssembly_Logs_Save()
        {
            var module = new Module(GetType().Assembly.Location);
            string fileName = Path.GetFileName(module.AssemblyLocation);

            string path;
            using (var testDirectory = new TestDirectory())
            {
                testDirectory.SaveAssembly(module);
                path = testDirectory.FullName;
            }

            string expectedMessage = string.Format("DEBUG|Writing assembly \"{0}\" to \"{1}\".|", fileName, path);
            AssertLogContains(expectedMessage);
        }
	}
}

