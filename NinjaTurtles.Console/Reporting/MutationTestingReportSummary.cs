using System.Collections.Generic;
using System.Text;
using NinjaTurtles.Reporting;

namespace NinjaTurtles.Console.Reporting
{
    internal class MutationTestingReportSummary
    {
        private int _mutantsCount;
        private int _mutantsKilledCount;

        private readonly List<string> _methodsWithNoTest;

        public MutationTestingReportSummary()
        {
            _mutantsCount = 0;
            _mutantsKilledCount = 0;
            _methodsWithNoTest = new List<string>();
        }

        public int MutantsCount { get { return _mutantsCount; } }

        public int MutantsKilledCount { get { return _mutantsKilledCount; } } 

        public void MergeMutationTestReport(MutationTestingReport report)
        {
            _mutantsCount += report.MutantsCount;
            _mutantsKilledCount += report.MutantsKilledCount;

            if (!report.TestsFounded)
                _methodsWithNoTest.Add(report.MethodFullname);
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

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            double mutationScore = GetMutationScore();

            MethodWithNoTestsToStringHelper(builder);
            builder.AppendFormat("Mutation score: {0:0.##} ({1} / {2})", mutationScore, _mutantsKilledCount, _mutantsCount);
            return builder.ToString();
        }
    }
}
