using NinjaTurtlesMutation.Reporting;

namespace NinjaTurtlesMutation.Console.Reporting
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
