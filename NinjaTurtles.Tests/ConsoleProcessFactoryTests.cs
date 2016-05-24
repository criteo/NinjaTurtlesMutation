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

using System.IO;
using NinjaTurtlesMutation;
using NUnit.Framework;

namespace NinjaTurtles.Tests
{
    [TestFixture]
    public class ConsoleProcessFactoryTests
    {
        [Test]
        public void CreateProcess_Redirects_Standard_Output()
        {
            using (var process = ConsoleProcessFactory.CreateProcess("cmd.exe", ""))
            {
                Assert.IsFalse(process.StartInfo.UseShellExecute);
                Assert.IsTrue(process.StartInfo.RedirectStandardOutput);
            }
        }

        [Test]
        public void CreateProcess_Hides_Window()
        {
            using (var process = ConsoleProcessFactory.CreateProcess("cmd.exe", ""))
            {
                Assert.IsTrue(process.StartInfo.CreateNoWindow);
            }
        }

        [Test]
        public void CreateProcess_Gives_Correct_Information_For_CLR()
        {
            bool isMono = ConsoleProcessFactory.IsMono;
            ConsoleProcessFactory.IsMono = false;
            using (var process = ConsoleProcessFactory.CreateProcess("cmd.exe", ""))
            {
                ConsoleProcessFactory.IsMono = isMono;
                StringAssert.Contains("cmd.exe", process.StartInfo.FileName);
            }
        }

        [Test]
        public void CreateProcess_Gives_Correct_Information_For_Mono()
        {
            bool isMono = ConsoleProcessFactory.IsMono;
            ConsoleProcessFactory.IsMono = true;
            using (var process = ConsoleProcessFactory.CreateProcess("notfound.exe", "args"))
            {
                ConsoleProcessFactory.IsMono = isMono;
                Assert.AreEqual("mono", process.StartInfo.FileName);
                StringAssert.StartsWith("--runtime=v4.0 \"notfound.exe\" args", process.StartInfo.Arguments);
            }
        }

        [Test]
        public void CreateProcess_Resolves_Full_Exe_Path()
        {
            string exeName = ConsoleProcessFactory.IsWindows ? "cmd.exe" : "mono";
            bool isMono = ConsoleProcessFactory.IsMono;
            ConsoleProcessFactory.IsMono = false;
            using (var process = ConsoleProcessFactory.CreateProcess(exeName, ""))
            {
                ConsoleProcessFactory.IsMono = isMono;
                Assert.AreNotEqual(exeName, process.StartInfo.FileName);
                Assert.IsTrue(File.Exists(process.StartInfo.FileName));
            }
        }

        [Test]
        public void CreateProcess_Returns_Exe_Name_Verbatim_If_Not_Found()
        {
            string exeName = "icantfindyou.exe";
            using (var process = ConsoleProcessFactory.CreateProcess(exeName, ""))
            {
                Assert.AreEqual(exeName, process.StartInfo.FileName);
            }
        }

        [Test]
        public void CreateProcess_Uses_Provided_Search_Path()
        {
            string tempFile = Path.GetTempFileName();
            string exeName = Path.GetFileName(tempFile);
            using (var process = ConsoleProcessFactory.CreateProcess(exeName, "", new[] {Path.GetTempPath()}))
            {
                Assert.AreNotEqual(exeName, process.StartInfo.FileName);
                Assert.IsTrue(File.Exists(process.StartInfo.FileName));
            }
            File.Delete(tempFile);
        }

        [Test]
        public void CreateProcess_Uses_Correct_Switch_Format_For_Windows()
        {
            bool isWindows = ConsoleProcessFactory.IsWindows;
            ConsoleProcessFactory.IsWindows = true;
            using (var process = ConsoleProcessFactory.CreateProcess("cmd.exe", "{0}arg=val"))
            {
                ConsoleProcessFactory.IsWindows = isWindows;
                StringAssert.Contains("/arg=val", process.StartInfo.Arguments);
            }
        }

        [Test]
        public void CreateProcess_Uses_Correct_Switch_Format_For_Non_Windows()
        {
            bool isWindows = ConsoleProcessFactory.IsWindows;
            ConsoleProcessFactory.IsWindows = false;
            using (var process = ConsoleProcessFactory.CreateProcess("cmd.exe", "{0}arg=val"))
            {
                ConsoleProcessFactory.IsWindows = isWindows;
                StringAssert.Contains("-arg=val", process.StartInfo.Arguments);
            }
        }

        [Test]
        public void CreateProcess_Sets_Correct_Properties()
        {
            using (var process = ConsoleProcessFactory.CreateProcess("cmd.exe", ""))
            {
                Assert.IsFalse(process.StartInfo.UseShellExecute);
                Assert.IsTrue(process.StartInfo.CreateNoWindow);
                Assert.IsTrue(process.StartInfo.RedirectStandardOutput);
            }
        }
    }
}
