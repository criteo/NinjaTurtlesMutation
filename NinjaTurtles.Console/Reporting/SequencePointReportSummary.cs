using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaTurtles.Reporting;

namespace NinjaTurtles.Console.Reporting
{
    internal class SequencePointReportSummary
    {
        public int StartLine { get; set; }

        public SequencePointReportSummary(SequencePoint sequencePoint)
        {
            StartLine = sequencePoint.StartLine;
            MergeAppliedMutant(sequencePoint);
        }

        public void MergeAppliedMutant(SequencePoint sequencePoint)
        {
            if (sequencePoint.StartLine != StartLine)
                throw new Exception("Merging non matching sequence point summary");
            return;
        }
    }
}
