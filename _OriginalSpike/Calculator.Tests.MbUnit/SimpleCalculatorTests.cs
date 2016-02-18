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
// License along with Refix.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System;

using MbUnit.Framework;

using NinjaTurtles;
using NinjaTurtles.Attributes;
using NinjaTurtles.TestRunner;
using NinjaTurtles.Turtles.Method;

namespace Calculator.Tests.MbUnit
{
    [TestFixture]
    public class SimpleCalculatorTests
    {
        [FixtureSetUp]
        public void FixtureSetUp()
        {
            MutationTestBuilder.UseRunner<GallioTestRunner>();
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
            MutationTestBuilder.Clear();
        }

        [Test]
        [Row(3, 4, 7)]
        [Row(3, 0, 3)]
        [MethodTested(typeof(SimpleCalculator), "Add")]
        public void Add_SimpleTests(int left, int right, int result)
        {
            Assert.AreEqual(result, new SimpleCalculator().Add(left, right));
        }
		
		[Test]
        [Row(3, 4, 7)]
        [Row(3, 0, 3)]
        [MethodTested(typeof(SimpleCalculator), "StaticAdd")]
        public void StaticAdd_SimpleTests(int left, int right, int result)
        {
            Assert.AreEqual(result, SimpleCalculator.StaticAdd(left, right));
        }
		
		[Test]
        [Row(1, 2, 3, 4, 10)]
        [MethodTested(typeof(SimpleCalculator), "MultiAdd")]
        public void MultiAdd_SimpleTests(int i1, int i2, int i3, int i4, int result)
        {
            Assert.AreEqual(result, new SimpleCalculator().MultiAdd(i1, i2, i3, i4));
        }
		
		[Test]
        [Row(1, 2, 3, 4, 5, 7, 22)]
        [MethodTested(typeof(SimpleCalculator), "MixedAdd")]
        public void MixedAdd_SimpleTests(short i1, short i2, short i3, int i4, int i5, int i6, int result)
        {
            Assert.AreEqual(result, new SimpleCalculator().MixedAdd(i1, i2, i3, i4, i5, i6));
        }
		
		[Test]
        [Row(4, 2, 2)]
        [Row(3, 2, 1)]
        [Row(-8, 2, -4)]
        [MethodTested(typeof(SimpleCalculator), "Divide")]
        public void Divide_SimpleTests(int left, int right, int result)
        {
            Assert.AreEqual(result, new SimpleCalculator().Divide(left, right));
        }

        [Test]
        [MethodTested(typeof(SimpleCalculator), "Divide")]
        public void Divide_DivideByZero()
        {
            Assert.Throws<ArgumentException>(() => new SimpleCalculator().Divide(1, 0));
        }

        [Test]
        [MethodTested(typeof(SimpleCalculator), "AddViaMethodChainAndLinq")]
        [MethodTested(typeof(SimpleCalculator), "Sum")]
        [Row(3, 4, 5, 12)]
        public void Sum_SimpleTests(int i1, int i2, int i3, int result)
        {
            Assert.AreEqual(result, new SimpleCalculator().AddViaMethodChainAndLinq(i1, i2, i3));
        }

        [Test, Category("Mutation")]
        public void Add_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Add")
                .Run();
        }

        [Test, Category("Mutation")]
        public void StaticAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("StaticAdd")
                .Run();
        }

        [Test, Category("Mutation")]
        public void MultiAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("MultiAdd")
                .Run();
        }

        [Test, Category("Mutation")]
        public void MixedAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("MixedAdd")
                .With<ParameterAndVariableReadSubstitutionTurtle>()
                .With<VariableWriteSubstitutionTurtle>()
                .Run();
        }

        [Test, Category("Mutation")]
        public void Divide_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Divide")
                .Run();
        }

        [Test, Category("Mutation")]
        public void AddViaMethodChainAndLinq_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("AddViaMethodChainAndLinq")
                .Run();
        }

        [Test, Category("Mutation")]
        public void Sum_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Sum")
                .Run();
        }

    }
}
