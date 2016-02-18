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

using NinjaTurtles;
using NinjaTurtles.Attributes;
using NinjaTurtles.TestRunner;
using NinjaTurtles.Turtles.Method;

using Xunit;
using Xunit.Extensions;

namespace Calculator.Tests.NUnit
{
    public class SimpleCalculatorTests : IDisposable
    {
        public SimpleCalculatorTests()
        {
            MutationTestBuilder.UseRunner<xUnitTestRunner>();
        }

        public void Dispose()
        {
            MutationTestBuilder.Clear();
        }

        [Theory]
        [InlineData(3, 4, 7)]
        [InlineData(3, 0, 3)]
        [MethodTested(typeof(SimpleCalculator), "Add")]
        public void Add_SimpleTests(int left, int right, int result)
        {
            Assert.Equal(result, new SimpleCalculator().Add(left, right));
        }

        [Theory]
        [InlineData(3, 4, 7)]
        [InlineData(3, 0, 3)]
        [MethodTested(typeof(SimpleCalculator), "StaticAdd")]
        public void StaticAdd_SimpleTests(int left, int right, int result)
        {
            Assert.Equal(result, SimpleCalculator.StaticAdd(left, right));
        }

        [Theory]
        [InlineData(1, 2, 3, 4, 10)]
        [MethodTested(typeof(SimpleCalculator), "MultiAdd")]
        public void MultiAdd_SimpleTests(int i1, int i2, int i3, int i4, int result)
        {
            Assert.Equal(result, new SimpleCalculator().MultiAdd(i1, i2, i3, i4));
        }

        [Theory]
        [InlineData((short)1, (short)2, (short)3, 4, 5, 7, 22)]
        [MethodTested(typeof(SimpleCalculator), "MixedAdd")]
        public void MixedAdd_SimpleTests(short i1, short i2, short i3, int i4, int i5, int i6, int result)
        {
            Assert.Equal(result, new SimpleCalculator().MixedAdd(i1, i2, i3, i4, i5, i6));
        }

        [Theory]
        [InlineData(4, 2, 2)]
        [InlineData(3, 2, 1)]
        [InlineData(-8, 2, -4)]
        [MethodTested(typeof(SimpleCalculator), "Divide")]
        public void Divide_SimpleTests(int left, int right, int result)
        {
            Assert.Equal(result, new SimpleCalculator().Divide(left, right));
        }

        [Fact]
        [MethodTested(typeof(SimpleCalculator), "Divide")]
        public void Divide_DivideByZero()
        {
            Assert.Throws<ArgumentException>(() => new SimpleCalculator().Divide(1, 0));
        }

        [Theory]
        [MethodTested(typeof(SimpleCalculator), "AddViaMethodChainAndLinq")]
        [MethodTested(typeof(SimpleCalculator), "Sum")]
        [InlineData(3, 4, 5, 12)]
        public void Sum_SimpleTests(int i1, int i2, int i3, int result)
        {
            Assert.Equal(result, new SimpleCalculator().AddViaMethodChainAndLinq(i1, i2, i3));
        }

        [Fact, Trait("Category", "Mutation")]
        public void Add_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Add")
                .Run();
        }

        [Fact, Trait("Category", "Mutation")]
        public void StaticAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("StaticAdd")
                .Run();
        }

        [Fact, Trait("Category", "Mutation")]
        public void MultiAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("MultiAdd")
                .Run();
        }

        [Fact, Trait("Category", "Mutation")]
        public void MixedAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("MixedAdd")
                .With<ParameterAndVariableReadSubstitutionTurtle>()
                .With<VariableWriteSubstitutionTurtle>()
                .Run();
        }

        [Fact, Trait("Category", "Mutation")]
        public void Divide_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Divide")
                .Run();
        }

        [Fact, Trait("Category", "Mutation")]
        public void AddViaMethodChainAndLinq_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("AddViaMethodChainAndLinq")
                .Run();
        }

        [Fact, Trait("Category", "Mutation")]
        public void Sum_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Sum")
                .Run();
        }

    }
}
