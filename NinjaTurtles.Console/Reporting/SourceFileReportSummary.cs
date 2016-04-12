using System;
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

        public SourceFileReportSummary(SourceFile sourceFile)
        {
            Url = sourceFile.Url;
            Filename = sourceFile.FileName;
        }
    }
}
