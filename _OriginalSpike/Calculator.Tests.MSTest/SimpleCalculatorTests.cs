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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NinjaTurtles;
using NinjaTurtles.Attributes;
using NinjaTurtles.TestRunner;
using NinjaTurtles.Turtles.Method;

namespace Calculator.Tests.MSTest
{
    [TestClass]
    [Ignore]
    public class SimpleCalculatorTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            MutationTestBuilder.UseRunner<MSTestTestRunner>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MutationTestBuilder.Clear();
        }

        [TestMethod]
        [MethodTested(typeof(SimpleCalculator), "Add")]
        public void Add_SimpleTests()
        {
            Assert.AreEqual(7, new SimpleCalculator().Add(3, 4));
            Assert.AreEqual(3, new SimpleCalculator().Add(3, 0));
        }

        [TestMethod]
        [MethodTested(typeof(SimpleCalculator), "StaticAdd")]
        public void StaticAdd_SimpleTests()
        {
            Assert.AreEqual(7, SimpleCalculator.StaticAdd(3, 4));
            Assert.AreEqual(7, SimpleCalculator.StaticAdd(3, 4));
        }

        [TestMethod]
        [MethodTested(typeof(SimpleCalculator), "MultiAdd")]
        public void MultiAdd_SimpleTests()
        {
            Assert.AreEqual(10, new SimpleCalculator().MultiAdd(1, 2, 3, 4));
        }

        [TestMethod]
        [MethodTested(typeof(SimpleCalculator), "MixedAdd")]
        public void MixedAdd_SimpleTests()
        {
            Assert.AreEqual(22, new SimpleCalculator().MixedAdd(1, 2, 3, 4, 5, 7));
        }

        [TestMethod]
        [MethodTested(typeof(SimpleCalculator), "Divide")]
        public void Divide_SimpleTests()
        {
            Assert.AreEqual(2, new SimpleCalculator().Divide(4, 2));
            Assert.AreEqual(1, new SimpleCalculator().Divide(3, 2));
            Assert.AreEqual(-4, new SimpleCalculator().Divide(-8, 2));
        }

        [TestMethod]
        [MethodTested(typeof(SimpleCalculator), "Divide")]
        [ExpectedException(typeof(ArgumentException))]
        public void Divide_DivideByZero()
        {
            new SimpleCalculator().Divide(1, 0);
        }

        [TestMethod]
        [MethodTested(typeof(SimpleCalculator), "AddViaMethodChainAndLinq")]
        [MethodTested(typeof(SimpleCalculator), "Sum")]
        public void Sum_SimpleTests()
        {
            Assert.AreEqual(12, new SimpleCalculator().AddViaMethodChainAndLinq(3, 4, 5));
        }

        [TestMethod, TestCategory("Mutation")]
        public void Add_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Add")
                .Run();
        }

        [TestMethod, TestCategory("Mutation")]
        public void StaticAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("StaticAdd")
                .Run();
        }

        [TestMethod, TestCategory("Mutation")]
        public void MultiAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("MultiAdd")
                .Run();
        }

        [TestMethod, TestCategory("Mutation")]
        public void MixedAdd_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("MixedAdd")
                .With<ParameterAndVariableReadSubstitutionTurtle>()
                .With<VariableWriteSubstitutionTurtle>()
                .Run();
        }

        [TestMethod, TestCategory("Mutation")]
        public void Divide_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Divide")
                .Run();
        }

        [TestMethod, TestCategory("Mutation")]
        public void AddViaMethodChainAndLinq_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("AddViaMethodChainAndLinq")
                .Run();
        }

        [TestMethod, TestCategory("Mutation")]
        public void Sum_MutationTests()
        {
            MutationTestBuilder<SimpleCalculator>.For("Sum")
                .Run();
        }

    }
}
