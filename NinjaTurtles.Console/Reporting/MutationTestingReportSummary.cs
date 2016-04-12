using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NinjaTurtles.Reporting;

namespace NinjaTurtles.Console.Reporting
{
    internal class MutationTestingReportSummary
    {
        private int _mutantsCount;
        private int _mutantsKilledCount;

        private readonly IList<string> _methodsWithNoTest;

        private readonly IList<SourceFileReportSummary> _survivingMutantsSources;

        public MutationTestingReportSummary()
        {
            _mutantsCount = 0;
            _mutantsKilledCount = 0;
            _methodsWithNoTest = new List<string>();
            _survivingMutantsSources = new List<SourceFileReportSummary>();
        }

        public int MutantsCount { get { return _mutantsCount; } }

        public int MutantsKilledCount { get { return _mutantsKilledCount; } }

        private void MergeSourceFileReport(MutationTestingReport report)
        {
            foreach (var sourceFile in report.SourceFiles)
            {
                if (sourceFile.SequencePoints.All(sp => sp.AppliedMutants.All(am => am.Killed)))
                    continue;
                var matchingSourceFile = _survivingMutantsSources.FirstOrDefault(sf => sf.Url == sourceFile.Url);
                if (matchingSourceFile == null)
                    _survivingMutantsSources.Add(new SourceFileReportSummary(sourceFile));
                else
                    matchingSourceFile.MergeSequencePoints(sourceFile);
            }
        }

        public void MergeMutationTestReport(MutationTestingReport report)
        {
            if (!report.TestsFounded)
            {
                _methodsWithNoTest.Add(report.MethodFullname);
                return;
            }
            _mutantsCount += report.MutantsCount;
            _mutantsKilledCount += report.MutantsKilledCount;
            MergeSourceFileReport(report);
        }

        public double GetMutationScore()
        {
            return (_mutantsCount > 0 ? _mutantsKilledCount/(double) _mutantsCount : 0);
        }

        private void MethodWithNoTestsToStringHelper(StringBuilder builder)
        {
            var methodsWithNoTestCount = _methodsWithNoTest.Count;
            if (methodsWithNoTestCount == 0)
                return;
            builder.AppendFormat("{0} method{1} have no matching test:\n", methodsWithNoTestCount, (methodsWithNoTestCount > 1 ? "s" : ""));
            foreach (var methodName in _methodsWithNoTest)
                builder.AppendFormat("\t{0}\n", methodName);
        }

        private void SurvivingMutantsSummaryToStringHelper(StringBuilder builder)
        {
            if (_survivingMutantsSources.Count == 0)
                return;
            builder.Append("Surviving mutants:\n");
            foreach (var sourceFile in _survivingMutantsSources)
            {
                builder.AppendFormat("  {0}\n", sourceFile.Url);
                foreach (var sequencePoint in sourceFile.SequencePoints.OrderBy(sp => sp.StartLine))
                {
                    builder.AppendFormat("      Line {0,-3}\n", sequencePoint.StartLine);
                    IEnumerable<IGrouping<string, MutantReportSummary>> groupedMutants = sequencePoint.MutantReports.GroupBy(mutant => mutant.GenericDescription);
                    foreach (IGrouping<string, MutantReportSummary> mutantsReportsGrouped in groupedMutants)
                    {
                        builder.AppendFormat("          {0} {1}\n", mutantsReportsGrouped.Key.PadRight(65), "(" + mutantsReportsGrouped.Count() + ")");
                        foreach (var mutantReport in mutantsReportsGrouped)
                            builder.AppendFormat("              {0}\n", mutantReport.Description);
                        builder.Append("\n");
                    }
                    builder.Append("\n");
                }
                builder.Append("\n");
            }
            builder.Append("\n");
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            double mutationScore = GetMutationScore();

            MethodWithNoTestsToStringHelper(builder);
            SurvivingMutantsSummaryToStringHelper(builder);
            builder.AppendFormat("Mutation score: {0:0.##} ({1} / {2})", mutationScore, _mutantsKilledCount, _mutantsCount);
            return builder.ToString();
        }
    }
}
