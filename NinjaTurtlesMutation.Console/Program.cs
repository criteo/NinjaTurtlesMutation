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

using NinjaTurtlesMutation.Console.Options;

namespace NinjaTurtlesMutation.Console
{
    class Program
    {
        private const int SUCCESS = 0;
        private const int PARSE_FAILURE = 1;
        private const int VALIDATION_FAILURE = 2;
        private const int EXECUTION_FAILURE = 3;

        static int Main(string[] args)
        {
            CommandWithOptions options;
            try
            {
                options = new CommandWithOptions(args);
            }
            catch (ArgumentException ae)
            {
                System.Console.Error.WriteLine(ae.Message);
                return PARSE_FAILURE;
            }
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
