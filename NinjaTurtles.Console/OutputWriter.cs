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
using System.Globalization;
using System.IO;
using System.Text;

namespace NinjaTurtles.Console
{
    static internal class OutputWriter
    {
        static private bool _firstWritten;

        static OutputWriter()
        {
            Writer = new StreamWriter(System.Console.OpenStandardOutput(), Encoding.GetEncoding(437));
            Verbository = OutputVerbosity.Normal;
        }

        static public TextWriter Writer { get; set; }
        static public OutputVerbosity Verbository { get; set; }

        static public OutputWriterHighlight Highlight(ConsoleColor color, bool allow)
        {
            return new OutputWriterHighlight(allow ? color : System.Console.ForegroundColor);
        }

        static public OutputWriterHighlight Highlight(ConsoleColor color)
        {
            return Highlight(color, true);
        }

        static public OutputWriterBackgroundHighlight BackgroundHighlight(ConsoleColor color, bool allow)
        {
            return new OutputWriterBackgroundHighlight(allow ? color : System.Console.ForegroundColor);
        }

        static public OutputWriterBackgroundHighlight BackgroundHighlight(ConsoleColor color)
        {
            return BackgroundHighlight(color, true);
        }

        static public void WriteLine()
        {
            WriteLine("");
        }

        static public void WriteLine(string format, params object[] args)
        {
            WriteLine(OutputVerbosity.Normal, format, args);
        }

        static public void WriteLine(OutputVerbosity level)
        {
            WriteLine(level, "");
        }

        static public void Write(string message)
        {
            Write(OutputVerbosity.Normal, message);
        }

        static public void Write(OutputVerbosity level, string format, params object[] args)
        {
            string message = string.Format(CultureInfo.CurrentUICulture, format, args);
            if (Verbository >= level)
            {
                if (!_firstWritten)
                {
                    Writer.WriteLine();
                    _firstWritten = true;
                }
                Writer.Write(message);
                Writer.Flush();
            }
        }

        static public void WriteLine(OutputVerbosity level, string format, params object[] args)
        {
            string message = string.Format(CultureInfo.CurrentUICulture, format, args);
            if (Verbository >= level)
            {
                if (!_firstWritten)
                {
                    Writer.WriteLine();
                    _firstWritten = true;
                }
                Writer.WriteLine(message);
                Writer.Flush();
            }
        }
    }
}
