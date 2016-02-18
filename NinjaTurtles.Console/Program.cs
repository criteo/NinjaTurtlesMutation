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

namespace NinjaTurtles.Console
{
    class Program
    {
        private const int SUCCESS = 0;
        private const int VALIDATION_FAILURE = 1;
        private const int EXECUTION_FAILURE = 2;

        static int Main(string[] args)
        {
            var options = new CommandWithOptions(args);
            if (!options.Command.Validate())
            {
                return VALIDATION_FAILURE;
            }
            bool success = options.Command.Execute();
            OutputWriter.WriteLine(OutputVerbosity.Normal);
            if (!success)
            {
                return EXECUTION_FAILURE;
            }
            return SUCCESS;
        }
    }
}
