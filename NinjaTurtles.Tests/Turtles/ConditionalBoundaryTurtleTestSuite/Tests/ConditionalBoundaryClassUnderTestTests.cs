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

namespace NinjaTurtles.Tests.Turtles.ConditionalBoundaryTurtleTestSuite.Tests
{
    [TestFixture]
    public class ConditionalBoundaryClassUnderTestTests
    {
        [Test]
        public void IsNegative_Works()
        {
            Assert.IsTrue(new ConditionalBoundaryClassUnderTest().IsNegative(-1));
            Assert.IsFalse(new ConditionalBoundaryClassUnderTest().IsNegative(1));
        }

        [Test]
        public void WorkingIsNegative_Works()
        {
            Assert.IsTrue(new ConditionalBoundaryClassUnderTest().WorkingIsNegative(-1));
            Assert.IsFalse(new ConditionalBoundaryClassUnderTest().WorkingIsNegative(1));
            Assert.IsFalse(new ConditionalBoundaryClassUnderTest().WorkingIsNegative(0));
        }

        [Test]
        public void IsPositive_Works()
        {
            Assert.IsFalse(new ConditionalBoundaryClassUnderTest().IsPositive(-1));
            Assert.IsTrue(new ConditionalBoundaryClassUnderTest().IsPositive(1));
        }

        [Test]
        public void WorkingIsPositive_Works()
        {
            Assert.IsFalse(new ConditionalBoundaryClassUnderTest().WorkingIsPositive(-1));
            Assert.IsTrue(new ConditionalBoundaryClassUnderTest().WorkingIsPositive(1));
            Assert.IsFalse(new ConditionalBoundaryClassUnderTest().WorkingIsPositive(0));
        }

        [Test]
        public void IsNegative_Mutation_Tests_Fail()
        {
            try
            {
                MutationTestBuilder<ConditionalBoundaryClassUnderTest>
                    .For("IsNegative")
                    .With<ConditionalBoundaryTurtle>()
                    .Run();
            }
            catch (MutationTestFailureException)
            {
                return;
            }
            Assert.Fail("MutationTestFailureException was not thrown.");
        }

        [Test]
        public void WorkingIsNegative_Mutation_Tests_Pass()
        {
            MutationTestBuilder<ConditionalBoundaryClassUnderTest>
                .For("WorkingIsNegative")
                .With<ConditionalBoundaryTurtle>()
                .Run();
        }

        [Test]
        public void IsPositive_Mutation_Tests_Fail()
        {
            try
            {
                MutationTestBuilder<ConditionalBoundaryClassUnderTest>
                    .For("IsPositive")
                    .With<ConditionalBoundaryTurtle>()
                    .Run();
            }
            catch (MutationTestFailureException)
            {
                return;
            }
            Assert.Fail("MutationTestFailureException was not thrown.");
        }

        [Test]
        public void WorkingIsPositive_Mutation_Tests_Pass()
        {
            MutationTestBuilder<ConditionalBoundaryClassUnderTest>
                .For("WorkingIsPositive")
                .With<ConditionalBoundaryTurtle>()
                .Run();
        }
    }
}
