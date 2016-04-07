using System.Text;
using NinjaTurtles.Reporting;

namespace NinjaTurtles.Console.Reporting
{
    internal class MutationTestingReportSummary
    {
        private int _mutantsCount;
        private int _mutantsKilledCount;

        public MutationTestingReportSummary()
        {
            _mutantsCount = 0;
            _mutantsKilledCount = 0;
        }

        public int MutantsCount { get { return _mutantsCount; } }

        public int MutantsKilledCount { get { return _mutantsKilledCount; } } 

        public void MergeMutationTestReport(MutationTestingReport report)
        {
            _mutantsCount += report.MutantsCount;
            _mutantsKilledCount += report.MutantsKilledCount;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            double mutationScore = _mutantsKilledCount/(double)_mutantsCount;

            builder.AppendFormat("Mutation score: {0} ({1} / {2})", mutationScore, _mutantsKilledCount, _mutantsCount);
            return builder.ToString();
        }
    }
}
