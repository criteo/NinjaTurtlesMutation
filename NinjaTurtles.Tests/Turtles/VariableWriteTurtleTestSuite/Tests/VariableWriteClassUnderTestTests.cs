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

using NinjaTurtles.Turtles;

namespace NinjaTurtles.Tests.Turtles.VariableWriteTurtleTestSuite.Tests
{
    [TestFixture]
    public class VariableWriteClassUnderTestTests
    {
        [Test]
        public void AddWithPointlessNonsense_Works()
        {
            Assert.AreEqual(4, new VariableWriteClassUnderTest().AddWithPointlessNonsense(1, 3));
            Assert.AreEqual(85, new VariableWriteClassUnderTest().AddWithPointlessNonsense(-7, 92));
        }

        [Test]
        public void AddWithPointlessNonsenseViaFields_Works()
        {
            Assert.AreEqual(4, new VariableWriteClassUnderTest().AddWithPointlessNonsenseViaFields(1, 3));
            Assert.AreEqual(85, new VariableWriteClassUnderTest().AddWithPointlessNonsenseViaFields(-7, 92));
        }

        [Test]
        public void AddWithPointlessNonsenseViaMixture_Works()
        {
            Assert.AreEqual(4, new VariableWriteClassUnderTest().AddWithPointlessNonsenseViaMixture(1, 3));
            Assert.AreEqual(85, new VariableWriteClassUnderTest().AddWithPointlessNonsenseViaMixture(-7, 92));
        }

        [Test]
        public void AddWithoutPointlessNonsense_Works()
        {
            Assert.AreEqual(4, new VariableWriteClassUnderTest().AddWithoutPointlessNonsense(1, 3));
            Assert.AreEqual(85, new VariableWriteClassUnderTest().AddWithoutPointlessNonsense(-7, 92));
        }
    }
}
