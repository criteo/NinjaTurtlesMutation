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

using System.Threading;
using NinjaTurtlesMutation;
using NUnit.Framework;


namespace NinjaTurtles.Tests
{
	[TestFixture]
	public class MutationTestTests
	{
        [Test, Category("Mutation"), MutationTest]
        public void LongRunning_Test_Method()
        {
            new LongRunningWhenMutated().LongRunning();
        }

        [TestFixture]
        public class NestedUnitTests
	    {
	        [Test]
            public void DoSomethingBanale_Works()
	        {
	            var nested = new OnlyTestedByNestedTestClass();
                Assert.AreEqual(0, nested.DoSomethingBanale(0));
                Assert.AreEqual(-2, nested.DoSomethingBanale(-1));
                Assert.AreEqual(2, nested.DoSomethingBanale(1));
                Assert.AreEqual(14, nested.DoSomethingBanale(7));
            }
	    }

        public class OnlyTestedByNestedTestClass
	    {
            public int DoSomethingBanale(int a)
            {
                return 2 * a;
            }
	    
	    }

	    public class LongRunningWhenMutated
	    {
            public void LongRunning()
            {
                Thread.Sleep(31000);
            }
	    }
	}
}

