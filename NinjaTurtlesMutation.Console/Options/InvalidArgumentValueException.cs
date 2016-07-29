using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTurtlesMutation.Console.Options
{
    internal class InvalidArgumentValueException : ArgumentException
    {
        public InvalidArgumentValueException(string optionname, string why, string argument) :
            base($"{optionname}: {why}: {argument}") { }
    }
}
