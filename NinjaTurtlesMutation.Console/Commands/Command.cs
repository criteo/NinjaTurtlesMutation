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
using System.Linq;
using NinjaTurtlesMutation.Console.Options;

namespace NinjaTurtlesMutation.Console.Commands
{
    internal abstract class Command
    {
        static public readonly IDictionary<string, Type> KnownCommands =
            new Dictionary<string, Type>
                {
                    {"clean", typeof(Clean)},
                    {"help", typeof(Help)},
                    {"run", typeof(Run)},
                };

        public CommandWithOptions Options { get; set; }
        protected abstract string HelpText { get; }

        static public bool TryParse(Queue<string> queue, out Command command)
        {
            command = null;
            if (queue.Count == 0)
            {
                using (new OutputWriterErrorHighlight())
                {
                    OutputWriter.WriteLine(
                        OutputVerbosity.Quiet,
                        @"You must specify a command.");
                }
                OutputWriter.WriteLine();
                return false;
            }
            string commandName = queue.Dequeue();
            return TryParse(commandName, out command);
        }

        static protected bool TryParse(string commandName, out Command command)
        {
            command = null;
            if (!KnownCommands.ContainsKey(commandName))
            {
                using (new OutputWriterErrorHighlight())
                {
                    OutputWriter.WriteLine(
                        OutputVerbosity.Quiet,
                        @"Unkown command '{0}' specified.",
                        commandName);
                }
                OutputWriter.WriteLine();
                return false;
            }
            command = (Command)Activator.CreateInstance(KnownCommands[commandName]);
            return true;
        }

        public virtual bool Validate()
        {
            ShowSplash();
            if (Options.Options.Any(o => o is Verbose)
                && Options.Options.Any(o => o is Quiet))
            {
                using (new OutputWriterErrorHighlight())
                {
                    OutputWriter.WriteLine(
                        OutputVerbosity.Quiet,
                        @"The specified combination of options is invalid.");
                }
                OutputWriter.WriteLine();
                ShowHelp();
                return false;
            }
            return true;
        }

        public abstract bool Execute();

        public void ShowSplash()
        {
            if (Options.Options.Any(o => o is NoSplash))
            {
                return;
            }
            using (new OutputWriterHighlight(ConsoleColor.White))
            {
                OutputWriter.WriteLine(
                    @"NinjaTurtles - mutation testing for .NET (version {0})",
                    typeof(Command).Assembly.GetName().Version);
                OutputWriter.WriteLine(
                    @"Copyright (C) 2016 Tony Roussel.");
            }
            OutputWriter.WriteLine();
        }

        public void ShowHelp()
        {
            OutputWriter.WriteLine(OutputVerbosity.Quiet, HelpText);
        }
    }
}
