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

using System.Collections.Generic;

using NinjaTurtles.Console.Commands;
using NinjaTurtles.Console.Options;

namespace NinjaTurtles.Console
{
    internal class CommandWithOptions
    {
        private readonly List<string> _arguments = new List<string>();
        private readonly Command _command;
        private readonly List<Option> _options = new List<Option>();

        public CommandWithOptions(Command command)
        {
            _command = command;
            _command.Options = this;
        }

        public CommandWithOptions(IEnumerable<string> args)
        {
            var queue = new Queue<string>(args);
            if (!Command.TryParse(queue, out _command))
            {
                _command = new Help { Options = this };
                return;
            }
            _command.Options = this;
            IEnumerable<Option> commandLineOptions;
            while (Option.TryParse(queue, out commandLineOptions))
            {
                _options.AddRange(commandLineOptions);
            }
            _arguments.AddRange(queue);
        }

        public Command Command
        {
            get { return _command; }
        }

        public IList<Option> Options
        {
            get { return _options; }
        }

        public IList<string> Arguments
        {
            get { return _arguments; }
        }
    }
}
