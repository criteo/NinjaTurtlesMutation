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

namespace NinjaTurtlesMutation.Console.Commands
{
    internal class Clean : Command
    {
        protected override string HelpText
        {
            get { return @"usage: NinjaTurtles.Console clean

Cleans any remaining NinjaTurtles test folders to free up disk space.

Example:
   NinjaTurtles.Console clean

   This command will attempt to delete any remaining NinjaTurtles test
   folders still present."; }
        }

        public override bool Validate()
        {
            if (!base.Validate())
            {
                return false;
            }
            if (Options.Arguments.Count != 0)
            {
                using (new OutputWriterErrorHighlight())
                {
                    OutputWriter.WriteLine(
                        OutputVerbosity.Quiet,
                        @"The 'clean' command does not take any arguments.");
                }
                return false;
            }
            return true;
        }

        public override bool Execute()
        {
            bool result = true;

            string testFolderLocation = Path.Combine(Path.GetTempPath(), "NinjaTurtlesMutation");
            var testFolder = new DirectoryInfo(testFolderLocation);
            int count = 0;
            int failures = 0;
            foreach (var subFolder in testFolder.GetDirectories())
            {
                count++;
                try
                {
                    subFolder.Delete(true);
                }
                catch
                {
                    failures++;
                    result = false;
                    OutputWriter.WriteLine(
                        OutputVerbosity.Verbose,
                        @"The folder {0} could not be deleted.",
                        subFolder.Name);
                }
            }

            OutputWriter.WriteLine(
                @"{0} folders found, {1} successfully deleted.",
                count, count - failures);

            return result;
        }
    }

}