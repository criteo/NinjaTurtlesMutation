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

namespace NinjaTurtlesMutation.Console.Options
{
    internal abstract class Option
    {
        public static readonly IDictionary<string, Type> KnownOptions =
            new Dictionary<string, Type>
                {
                    {"-verbose", typeof(Verbose)},
                    {"v", typeof(Verbose)},
                    {"-quiet", typeof(Quiet)},
                    {"q", typeof(Quiet)},
                    {"-no-splash", typeof(NoSplash)},
                    {"n", typeof(NoSplash)},
                    {"-class", typeof(TargetClass)},
                    {"c", typeof(TargetClass)},
                    /*{"-method", typeof(TargetMethod)},
                    {"m", typeof(TargetMethod)},*/
                    {"-output", typeof(Output)},
                    {"o", typeof(Output)},
                    {"-format", typeof(Format)},
                    {"f", typeof(Format)},
                    /*{"-type", typeof(ParameterType)},
                    {"t", typeof(ParameterType)},*/
                    {"-namespace", typeof(TargetNamespace)},
                    {"N", typeof(TargetNamespace)},
                    {"-parallelization", typeof(ParallelLevel)},
                    {"p", typeof(ParallelLevel)},
                    {"-no-pretest", typeof(NoPreTest)},
                };

        static public bool TryParse(Queue<string> queue, out IEnumerable<Option> commandLineOptions)
        {
            var options = new List<Option>();
            commandLineOptions = options;
            if (queue.Count > 0 && queue.Peek().StartsWith("-"))
            {
                string optionName = queue.Dequeue().Substring(1);
                Option option;
                if (optionName.StartsWith("-")) // Single verbose option
                {
                    if (!TryParseSingleOption(queue, optionName, out option))
                    {
                        return false;
                    }
                    options.Add(option);
                }
                else // Potentially multiple short from options
                {
                    foreach (char optionCharacter in optionName)
                    {
                        if (!TryParseSingleOption(queue, optionCharacter.ToString(), out option))
                        {
                            return false;
                        }
                        options.Add(option);
                    }
                }
                return true;
            }
            return false;
        }

        static private bool TryParseSingleOption(Queue<string> queue, string optionName,
                                                 out Option option)
        {
            option = null;
            if (!KnownOptions.ContainsKey(optionName))
            {
                OutputWriter.WriteLine(OutputVerbosity.Quiet,
                                       "Unknown option '{0}' specified.",
                                       optionName);
                return false;
            }
            option = (Option)Activator.CreateInstance(KnownOptions[optionName]);
            option.TakeArguments(queue);
            if (option is Quiet)
            {
                OutputWriter.Verbository = OutputVerbosity.Quiet;
            }
            if (option is Verbose)
            {
                OutputWriter.Verbository = OutputVerbosity.Verbose;
            }
            return true;
        }

        protected virtual void TakeArguments(Queue<string> queue) { }
    }
}
