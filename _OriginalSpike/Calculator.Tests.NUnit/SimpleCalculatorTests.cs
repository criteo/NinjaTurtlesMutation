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

using NUnit.Framework;

using NinjaTurtles;
using NinjaTurtles.Attributes;
using NinjaTurtles.Turtles.Method;

namespace Calculator.Tests.NUnit
{
    [TestFixture]
    public class SimpleCalculatorTests
    {
		[Test]
        [TestCase(3, 4, Result = 7)]
        [TestCase(3, 0, Result = 3)]
        [MethodTested(typeof(SimpleCalculator), "Add")]
        public int Add_SimpleTests(int left, int right)
        {
            return new SimpleCalculator().Add(left, right);
        }
		
		[Test]
        [TestCase(3, 4, Result = 7)]
        [TestCase(3, 0, Result = 3)]
        [MethodTested(typeof(SimpleCalculator), "StaticAdd")]
        public int StaticAdd_SimpleTests(int left, int right)
        {
            return SimpleCalculator.StaticAdd(left, right);
        }
		
		[Test]
        [TestCase(1, 2, 3, 4, Result = 10)]
        [MethodTested(typeof(SimpleCalculator), "MultiAdd")]
        public int MultiAdd_SimpleTests(int i1, int i2, int i3, int i4)
        {
            return new SimpleCalculator().MultiAdd(i1, i2, i3, i4);
        }
		
		[Test]
        [TestCase(1, 2, 3, 4, 5, 7, Result = 22)]
        [MethodTested(typeof(SimpleCalculator), "MixedAdd")]
        public int MixedAdd_SimpleTests(short i1, short i2, short i3, int i4, int i5, int i6)
        {
            return new SimpleCalculator().MixedAdd(i1, i2, i3, i4, i5, i6);
        }
		
		[Test]
        [TestCase(4, 2, Result = 2)]
        [TestCase(3, 2, Result = 1)]
        [TestCase(-8, 2, Result = -4)]
        [MethodTested(typeof(SimpleCalculator), "Divide")]
        public int Divide_SimpleTests(int left, int right)
        {
            return new SimpleCalculator().Divide(left, right);
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
        [TestCase(3, 4, 5, Result = 12)]
        public int Sum_SimpleTests(int i1, int i2, int i3)
        {
            return new SimpleCalculator().AddViaMethodChainAndLinq(i1, i2, i3);
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
