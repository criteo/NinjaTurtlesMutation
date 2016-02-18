#region Copyright & licence

// This file is part of NinjaTurtles.
// 
// NinjaTurtles is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// NinjaTurtles is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with NinjaTurtles.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012-14 David Musgrove and others.

#endregion

using NUnit.Framework;

using NinjaTurtles.Tests.TestUtilities;

namespace NinjaTurtles.Tests
{
    [TestFixture]
    public class TypeResolverLoggingTests : LoggingTestFixture
    {
        [Test]
        public void ResolveTypeFromReferences_Resolve_Within_Same_Assembly_Logs_Entry_And_One_Trace()
        {
            TypeResolver.ResolveTypeFromReferences(GetType().Assembly, "NinjaTurtles.Tests.TestUtilities.ConsoleCapturer");
            string debugMessage = "DEBUG|Resolving type \"NinjaTurtles.Tests.TestUtilities.ConsoleCapturer\" in \"NinjaTurtles.Tests\".|";
            string traceMessage = "TRACE|Searching for type \"NinjaTurtles.Tests.TestUtilities.ConsoleCapturer\" in \"NinjaTurtles.Tests\".|";
            string foundMessage = "TRACE|Found type \"NinjaTurtles.Tests.TestUtilities.ConsoleCapturer\" in \"NinjaTurtles.Tests\".|";
            AssertLogContains(debugMessage);
            AssertLogContains(traceMessage);
            AssertLogContains(foundMessage);
        }

        [Test]
        public void ResolveTypeFromReferences_Logs_If_Unrecognised()
        {
            TypeResolver.ResolveTypeFromReferences(GetType().Assembly, "System.NonexistentWidget");
            string notFoundMessage = "ERROR|Could not find type \"System.NonexistentWidget\".|";
            AssertLogContains(notFoundMessage);
        }

        [Test]
        public void ResolveTypeFromReferences_Resolves_Within_Referenced_Assembly_Logs_Entry_And_Traces()
        {
            TypeResolver.ResolveTypeFromReferences(GetType().Assembly, "System.Linq.ParallelEnumerable");
            string firstSearchMessage = "TRACE|Searching for type \"System.Linq.ParallelEnumerable\" in \"NinjaTurtles.Tests\".|";
            string secondSearchMessage = "TRACE|Searching for type \"System.Linq.ParallelEnumerable\" in \"System.Core\".|";
            string foundMessage = "TRACE|Found type \"System.Linq.ParallelEnumerable\" in \"System.Core\".|";
            AssertLogContains(firstSearchMessage);
            AssertLogContains(secondSearchMessage);
            AssertLogContains(foundMessage);
        }
    }
}
