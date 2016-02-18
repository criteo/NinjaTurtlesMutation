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

using Mono.Cecil.Cil;

using NUnit.Framework;

using NinjaTurtles.Turtles;

namespace NinjaTurtles.Tests.Turtles
{
    [TestFixture]
    public class VariableReadTurtleTests
    {
        [Test]
        public void DoMutate_Returns_Correct_Sequences()
        {
            var module = new Module(Assembly.GetExecutingAssembly().Location);
            module.LoadDebugInformation();
            var method = module.Definition
                .Types.Single(t => t.Name == "VariableReadClassUnderTest")
                .Methods.Single(t => t.Name == "AddAndDouble");

            var mutatedInstruction = method.Body.Instructions.First(i => i.OpCode == OpCodes.Ldarg_1);
            string hexPrefix = string.Format("{0:x4}: ", mutatedInstruction.Offset);

            var mutator = new VariableReadTurtle();
            IList<MutantMetaData> mutations = mutator
                .Mutate(method, module, method.Body.Instructions.Select(i => i.Offset).ToArray()).ToList();

            // V2 is only read for the return statement; this case is excluded in the code.
            Assert.AreEqual(9, mutations.Count);
            StringAssert.EndsWith(hexPrefix + "read substitution Int32.a => Int32.b", mutations[0].Description);
            StringAssert.EndsWith("read substitution Int32.a => Int32.total", mutations[1].Description);
            StringAssert.EndsWith("read substitution Int32.a => Int32.CS$1$0000", mutations[2].Description);
            StringAssert.EndsWith("read substitution Int32.b => Int32.a", mutations[3].Description);
            StringAssert.EndsWith("read substitution Int32.b => Int32.total", mutations[4].Description);
            StringAssert.EndsWith("read substitution Int32.b => Int32.CS$1$0000", mutations[5].Description);
            StringAssert.EndsWith("read substitution Int32.total => Int32.a", mutations[6].Description);
            StringAssert.EndsWith("read substitution Int32.total => Int32.b", mutations[7].Description);
            StringAssert.EndsWith("read substitution Int32.total => Int32.CS$1$0000", mutations[8].Description);
        }

        [Test]
        public void DoMutate_Returns_Correct_Sequences_Including_Field()
        {
            var module = new Module(Assembly.GetExecutingAssembly().Location);
            module.LoadDebugInformation();
            var method = module.Definition
                .Types.Single(t => t.Name == "VariableReadClassUnderTest")
                .Methods.Single(t => t.Name == "AddAndDoubleViaField");

            var mutator = new VariableReadTurtle();
            IList<MutantMetaData> mutations = mutator
                .Mutate(method, module, method.Body.Instructions.Select(i => i.Offset).ToArray()).ToList();

            // V2 is only read for the return statement; this case is excluded in the code.
            Assert.AreEqual(9, mutations.Count);
            StringAssert.EndsWith("read substitution Int32.a => Int32.b", mutations[0].Description);
            StringAssert.EndsWith("read substitution Int32.a => Int32.CS$1$0000", mutations[1].Description);
            StringAssert.EndsWith("read substitution Int32.a => Int32._total", mutations[2].Description);
            StringAssert.EndsWith("read substitution Int32.b => Int32.a", mutations[3].Description);
            StringAssert.EndsWith("read substitution Int32.b => Int32.CS$1$0000", mutations[4].Description);
            StringAssert.EndsWith("read substitution Int32.b => Int32._total", mutations[5].Description);
            StringAssert.EndsWith("read substitution Int32._total => Int32.a", mutations[6].Description);
            StringAssert.EndsWith("read substitution Int32._total => Int32.b", mutations[7].Description);
            StringAssert.EndsWith("read substitution Int32._total => Int32.CS$1$0000", mutations[8].Description);
        }

        [Test, Category("Mutation"), MutationTest]
        public void DoMutate_Mutation_Tests()
        {
            MutationTestBuilder<VariableReadTurtle>.For("DoMutate")
                .MergeReportTo("SampleReport.xml")
                .Run();
        }
    }
}
