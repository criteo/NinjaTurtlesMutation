using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTurtlesMutation.Console.Options
{
    internal class OptionArgumentParseException : ArgumentException
    {
        public OptionArgumentParseException(string optionname, string argument) :
            base(string.Format("{0}: couldn't parse argument: {1}", optionname, argument)) { }
    }
}
