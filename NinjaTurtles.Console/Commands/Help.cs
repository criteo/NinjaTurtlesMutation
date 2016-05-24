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

namespace NinjaTurtlesMutation.Console.Commands
{
    internal class Help : Command
    {
        protected override string HelpText
        {
            get { return GetHelpMessage(); }
        }

        public override bool Validate()
        {
            ShowSplash();

            // Help should always display.
            return true;
        }

        public override bool Execute()
        {
            if (Options.Arguments.Count > 0)
            {
                Command command;
                if (TryParse(Options.Arguments[0], out command))
                {
                    command.ShowHelp();
                    return true;
                }
            }

            var message = GetHelpMessage();
            OutputWriter.WriteLine(OutputVerbosity.Quiet, message);
            return true;
        }

        private static string GetHelpMessage()
        {
            const string template = @"usage: NinjaTurtles.Console <command> [options] [args]

NinjaTurtles console runner, version {0}.
Type 'NinjaTurtles.Console help <command>' for help on a specific command.

Available commands:
   help               : Displays general help on the NinjaTurtles console
                        runner tool.
   run                : Runs a set of mutation tests as specified by the
                        arguments and options provided.

Global options:
   --no-splash [-n]   : Suppress output of the NinjaTurtles splash header.
   --quiet [-q]       : Provides minimal output.
   --verbose [-v]     : Provides verbose output.";
            System.Version version = typeof(Command).Assembly.GetName().Version;
            string message = string.Format(template, version);
            return message;
        }
    }
}
