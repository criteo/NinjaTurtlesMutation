using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTurtlesMutation
{
    internal struct MutationTestInfo
    {
        public int MutantsCount;
        public int MutantsFailures;

        public MutationTestInfo(int count, int failures)
        {
            MutantsCount = count;
            MutantsFailures = failures;
        }

        public static MutationTestInfo operator +(MutationTestInfo linfo, MutationTestInfo rinfo)
        {
            return new MutationTestInfo(linfo.MutantsCount + rinfo.MutantsCount, linfo.MutantsFailures + rinfo.MutantsFailures);
        }
    }
}
