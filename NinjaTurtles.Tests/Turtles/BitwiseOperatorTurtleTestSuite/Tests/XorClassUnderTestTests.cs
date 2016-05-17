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
using NinjaTurtles.Turtles;

namespace NinjaTurtles.Tests.Turtles.BitwiseOperatorTurtleTestSuite.Tests
{
	[TestFixture]
	public class XorClassUnderTestTests
	{
		[Test]
		public void Dummy_Dummies()
		{
			Assert.AreEqual(0, new XorClassUnderTest().Dummy());
		}
		
		[Test]
        public void Xor_Works()
		{
			Assert.AreEqual(11, new XorClassUnderTest().Xor(3, 8));
		}
		
		[Test]
		public void WorkingXor_Works()
		{
			Assert.AreEqual(11, new XorClassUnderTest().WorkingXor(3, 8));
            Assert.AreEqual(4, new XorClassUnderTest().WorkingXor(3, 7));
		}
	}
}

