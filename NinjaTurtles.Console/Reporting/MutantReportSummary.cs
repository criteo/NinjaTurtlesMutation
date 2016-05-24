using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaTurtlesMutation.Reporting;

namespace NinjaTurtles.Console.Reporting
{
    internal class MutantReportSummary
    {
        public string GenericDescription { get; set; }

        public string Description { get; set; }

        public bool Killed { get; set; }

        public MutantReportSummary(AppliedMutant appliedMutant)
        {
            GenericDescription = appliedMutant.GenericDescription;
            Description = appliedMutant.Description;
            Killed = appliedMutant.Killed;
        }
    }
}
