using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTurtlesMutation.Console.Options
{
    internal abstract class ArgumentException : Exception
    {
        protected ArgumentException(string message) : base(message) { }
    }
}
