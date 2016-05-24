using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaTurtlesMutation.Reporting;

namespace NinjaTurtles.Console.Reporting
{
    internal class SequencePointReportSummary
    {
        public int StartLine { get; set; }

        public IList<MutantReportSummary> MutantReports;

        public SequencePointReportSummary(SequencePoint sequencePoint)
        {
            StartLine = sequencePoint.StartLine;
            MutantReports = new List<MutantReportSummary>();
            MergeAppliedMutant(sequencePoint);
        }

        public void MergeAppliedMutant(SequencePoint sequencePoint)
        {
            if (sequencePoint.StartLine != StartLine)
                throw new Exception("Merging non matching sequence point summary");
            foreach (var appliedMutant in sequencePoint.AppliedMutants.Where(am => !am.Killed))
            {
                if (MutantReports.Any(mr => mr.Description == appliedMutant.Description && mr.GenericDescription == appliedMutant.GenericDescription))
                    continue;
                MutantReports.Add(new MutantReportSummary(appliedMutant));
            }
        }
    }
}
