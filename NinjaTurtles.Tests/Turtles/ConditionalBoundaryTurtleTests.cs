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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using NinjaTurtlesMutation;
using NinjaTurtlesMutation.Turtles;
using NUnit.Framework;

namespace NinjaTurtles.Tests.Turtles
{
    [TestFixture]
    public class ConditionalBoundaryTurtleTests
    {
        private string _testFolder;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _testFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testFolder);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            Directory.Delete(_testFolder, true);
        }

        private static AssemblyDefinition CreateTestAssembly(OpCode opCode, bool invert)
        {
            var name = new AssemblyNameDefinition("TestConditionalsBoundaryTurtle", new Version(1, 0));
            var assembly = AssemblyDefinition.CreateAssembly(name, "TestClass", ModuleKind.Dll);
            var type = new TypeDefinition("TestConditionalsBoundaryTurtle", "TestClass",
                               TypeAttributes.Class | TypeAttributes.Public);
            var intType = assembly.MainModule.Import(typeof(int));
            var boolType = assembly.MainModule.Import(typeof(bool));
            var method = new MethodDefinition("TestMethod", MethodAttributes.Public, intType);
            var param = new ParameterDefinition("input", ParameterAttributes.In, intType);
            method.Parameters.Add(param);
            var resultVariable = new VariableDefinition(boolType);
            method.Body.Variables.Add(resultVariable);

            var processor = method.Body.GetILProcessor();
            Instruction loadReturnValueInstruction = processor.Create(OpCodes.Ldloc, resultVariable);
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldarg, param));
            method.Body.Instructions.Add(processor.Create(OpCodes.Ldc_I4, 0));
            method.Body.Instructions.Add(processor.Create(opCode));
            if (invert)
            {
                method.Body.Instructions.Add(processor.Create(OpCodes.Ldc_I4, 0));
                method.Body.Instructions.Add(processor.Create(OpCodes.Ceq));
            }
            method.Body.Instructions.Add(processor.Create(OpCodes.Stloc, resultVariable));
            method.Body.Instructions.Add(loadReturnValueInstruction);
            method.Body.Instructions.Add(processor.Create(OpCodes.Ret));

            type.Methods.Add(method);
            assembly.MainModule.Types.Add(type);
            return assembly;
        }

        private string GetTempAssemblyFileName()
        {
            return Path.Combine(_testFolder, "Test.dll");
        }

        [Test]
        public void DoMutate_Returns_Correct_Replacements_For_Less_Than()
        {
            var assembly = CreateTestAssembly(OpCodes.Clt, false);

            var addMethod = assembly.MainModule
                .Types.Single(t => t.Name == "TestClass")
                .Methods.Single(t => t.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);

            var mutator = new ConditionalBoundaryTurtle();
            int count = 0;
            foreach (var mutant in mutator
                .Mutate(addMethod, module, addMethod.Body.Instructions.Select(i => i.Offset).ToArray()))
            {
                ++count;
                StringAssert.EndsWith("Clt => not Cgt", mutant.Description);
                Assert.AreEqual(2, mutant.ILIndex);
                Assert.IsTrue(mutant.MethodDefinition.Body.Instructions[2]
                    .FollowsSequence(OpCodes.Cgt, OpCodes.Ldc_I4_0, OpCodes.Ceq, OpCodes.Stloc));
            }

            Assert.AreEqual(1, count); 
            Assert.IsTrue(addMethod.Body.Instructions[2]
                .FollowsSequence(OpCodes.Clt, OpCodes.Stloc_0));
        }

        [Test]
        public void DoMutate_Returns_Correct_Replacements_For_Less_Than_Or_Equal_To()
        {
            var assembly = CreateTestAssembly(OpCodes.Cgt, true);

            var addMethod = assembly.MainModule
                .Types.Single(t => t.Name == "TestClass")
                .Methods.Single(t => t.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);

            var mutator = new ConditionalBoundaryTurtle();
            int count = 0;
            foreach (var mutant in mutator
                .Mutate(addMethod, module, addMethod.Body.Instructions.Select(i => i.Offset).ToArray()))
            {
                ++count;
                StringAssert.EndsWith("Cgt => not Clt", mutant.Description);
                Assert.AreEqual(2, mutant.ILIndex);
                Assert.IsTrue(mutant.MethodDefinition.Body.Instructions[2]
                    .FollowsSequence(OpCodes.Clt, OpCodes.Ldc_I4_0, OpCodes.Ceq, OpCodes.Ldc_I4, OpCodes.Ceq, OpCodes.Stloc));
            }

            Assert.AreEqual(1, count);
            Assert.IsTrue(addMethod.Body.Instructions[2]
                .FollowsSequence(OpCodes.Cgt, OpCodes.Ldc_I4_0, OpCodes.Ceq, OpCodes.Stloc_0));
        }

        [Test]
        public void DoMutate_Returns_Correct_Replacements_For_Greater_Than()
        {
            var assembly = CreateTestAssembly(OpCodes.Cgt, false);

            var addMethod = assembly.MainModule
                .Types.Single(t => t.Name == "TestClass")
                .Methods.Single(t => t.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);

            var mutator = new ConditionalBoundaryTurtle();
            int count = 0;
            foreach (var mutant in mutator
                .Mutate(addMethod, module, addMethod.Body.Instructions.Select(i => i.Offset).ToArray()))
            {
                ++count;
                StringAssert.EndsWith("Cgt => not Clt", mutant.Description);
                Assert.AreEqual(2, mutant.ILIndex);
                Assert.IsTrue(mutant.MethodDefinition.Body.Instructions[2]
                    .FollowsSequence(OpCodes.Clt, OpCodes.Ldc_I4_0, OpCodes.Ceq, OpCodes.Stloc));
            }

            Assert.AreEqual(1, count);
            Assert.IsTrue(addMethod.Body.Instructions[2]
                .FollowsSequence(OpCodes.Cgt, OpCodes.Stloc_0));
        }

        [Test]
        public void DoMutate_Returns_Correct_Replacements_For_Greater_Than_Or_Equal_To()
        {
            var assembly = CreateTestAssembly(OpCodes.Clt, true);

            var addMethod = assembly.MainModule
                .Types.Single(t => t.Name == "TestClass")
                .Methods.Single(t => t.Name == "TestMethod");

            string tempAssemblyFileName = GetTempAssemblyFileName();
            assembly.Write(tempAssemblyFileName);
            var module = new Module(tempAssemblyFileName);

            var mutator = new ConditionalBoundaryTurtle();
            int count = 0;
            foreach (var mutant in mutator
                .Mutate(addMethod, module, addMethod.Body.Instructions.Select(i => i.Offset).ToArray()))
            {
                ++count;
                StringAssert.EndsWith("Clt => not Cgt", mutant.Description);
                Assert.AreEqual(2, mutant.ILIndex);
                Assert.IsTrue(mutant.MethodDefinition.Body.Instructions[2]
                    .FollowsSequence(OpCodes.Cgt, OpCodes.Ldc_I4_0, OpCodes.Ceq, OpCodes.Ldc_I4, OpCodes.Ceq, OpCodes.Stloc));
            }

            Assert.AreEqual(1, count);
            Assert.IsTrue(addMethod.Body.Instructions[2]
                .FollowsSequence(OpCodes.Clt, OpCodes.Ldc_I4_0, OpCodes.Ceq, OpCodes.Stloc_0));
        }
    }
}
