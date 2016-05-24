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

namespace NinjaTurtles.Tests.Turtles.VariableReadTurtleTestSuite.Tests
{
    [TestFixture]
    public class VariableReadClassUnderTestTests
    {
        [Test]
        public void AddAndDouble_Works()
        {
            Assert.AreEqual(0, new VariableReadClassUnderTest().AddAndDouble(0, 0));
            Assert.AreEqual(0, new VariableReadClassUnderTest().AddAndDouble(1, -1));
        }

        [Test]
        public void AddAndDoubleViaField_Works()
        {
            Assert.AreEqual(0, new VariableReadClassUnderTest().AddAndDoubleViaField(0, 0));
            Assert.AreEqual(0, new VariableReadClassUnderTest().AddAndDoubleViaField(1, -1));
        }

        [Test]
        public void WorkingAddAndDouble_Works()
        {
            Assert.AreEqual(0, new VariableReadClassUnderTest().WorkingAddAndDouble(0, 0));
            Assert.AreEqual(2, new VariableReadClassUnderTest().WorkingAddAndDouble(0, 1));
            Assert.AreEqual(4, new VariableReadClassUnderTest().WorkingAddAndDouble(1, 1));
            Assert.AreEqual(6, new VariableReadClassUnderTest().WorkingAddAndDouble(2, 1));
        }

        [Test]
        public void WorkingAddAndDoubleViaField_Works()
        {
            Assert.AreEqual(0, new VariableReadClassUnderTest().WorkingAddAndDoubleViaField(0, 0));
            Assert.AreEqual(2, new VariableReadClassUnderTest().WorkingAddAndDoubleViaField(0, 1));
            Assert.AreEqual(4, new VariableReadClassUnderTest().WorkingAddAndDoubleViaField(1, 1));
            Assert.AreEqual(6, new VariableReadClassUnderTest().WorkingAddAndDoubleViaField(2, 1));
        }
    }
}
