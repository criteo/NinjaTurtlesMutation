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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using NUnit.Framework;

using NinjaTurtles.Turtles;

namespace NinjaTurtles.Tests.Turtles
{
    [TestFixture]
    public class SequencePointDeletionTurtleTests
    {
        [Test]
        public void CreateMutant_Returns_Correct_Sequences()
        {
            var module = new Module(Assembly.GetExecutingAssembly().Location);
            module.LoadDebugInformation();
            var method = module.Definition
                .Types.Single(t => t.Name == "SequencePointDeletionClassUnderTest")
                .Methods.Single(t => t.Name == "SimpleMethod");

            var mutator = new SequencePointDeletionTurtle();
            IList<MutantMetaData> mutations = mutator
                .Mutate(method, module, method.Body.Instructions.Select(i => i.Offset).ToArray()).ToList();

            Assert.AreEqual(3, mutations.Count);
            StringAssert.EndsWith("deleting Ldarg, Stloc", mutations[0].Description);
            StringAssert.EndsWith("deleting Ldloc, Ldarg, Ldarg, Mul, Add, Stloc", mutations[1].Description);
            StringAssert.EndsWith("deleting Ldloc, Ldarg, Ldarg, Mul, Ldarg, Mul, Add, Stloc", mutations[2].Description);
        }

        [Test, Category("Mutation"), MutationTest]
        public void DoMutate_Mutation_Tests()
        {
            MutationTestBuilder<SequencePointDeletionTurtle>.For("DoMutate")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }

        [Test, Category("Mutation"), MutationTest]
        public void ShouldDeleteSequence_Mutation_Tests()
        {
            MutationTestBuilder<SequencePointDeletionTurtle>.For("ShouldDeleteSequence")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }
    }
}
