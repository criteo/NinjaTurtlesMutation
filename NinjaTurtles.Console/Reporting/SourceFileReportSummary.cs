using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaTurtles.Reporting;

namespace NinjaTurtles.Console.Reporting
{
    internal class SourceFileReportSummary
    {
        public string Url { get; private set; }

        public string Filename { get; set; }

        public SortedList<int, SequencePointReportSummary> SequencePoints;

        public SourceFileReportSummary(SourceFile sourceFile)
        {
            Url = sourceFile.Url;
            Filename = sourceFile.FileName;
            SequencePoints = new SortedList<int, SequencePointReportSummary>();
            MergeSequencePoints(sourceFile);
        }

        public void MergeSequencePoints(SourceFile sourceFile)
        {
            foreach (var sequencePoint in sourceFile.SequencePoints)
            {
                if (sequencePoint.AppliedMutants.All(am => am.Killed))
                    continue;
                var sequencePointAlreadyCreated = SequencePoints.ContainsKey(sequencePoint.StartLine);
                if (!sequencePointAlreadyCreated)
                    SequencePoints.Add(sequencePoint.StartLine, new SequencePointReportSummary(sequencePoint));
                else
                    SequencePoints[sequencePoint.StartLine].MergeAppliedMutant(sequencePoint);
            }
        }
    }
}
